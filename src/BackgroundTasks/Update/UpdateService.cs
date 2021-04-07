using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;
using Peoples = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.People;
using Included = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class UpdateService : IUpdateService
    {
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IUpdateRepository _updateRepository;

        private const long MayLeaveAloneFieldId = 438360;
        private const long HasPeopleWithoutPickupPermissionFieldId = 441655;

        private const int DaysLookBack = 7;
        
        public UpdateService(IPlanningCenterClient planningCenterClient, IUpdateRepository updateRepository)
        {
            _planningCenterClient = planningCenterClient;
            _updateRepository = updateRepository;
        }

        public async Task FetchDataFromPlanningCenter()
        {
            var checkIns = await _planningCenterClient.GetCheckedInPeople(daysLookBack: DaysLookBack).ConfigureAwait(continueOnCapturedContext: false);

            await UpdateLocations(checkIns: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            
            var preCheckIns = await FilterAndMapToPreCheckIns(checkIns: checkIns).ConfigureAwait(continueOnCapturedContext: false);

            await InsertNewPreCheckIns(preCheckIns: preCheckIns).ConfigureAwait(continueOnCapturedContext: false);

            await UpdatePeople().ConfigureAwait(continueOnCapturedContext: false);

            await AutoCheckInOutVolunteers().ConfigureAwait(continueOnCapturedContext: false);

            await _updateRepository.AutoCheckoutEveryoneByEndOfDay().ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task UpdateLocations(ImmutableList<CheckIns> checkIns)
        {
            var locations = checkIns.SelectMany(selector: c
                => c.Included?.Where(predicate: i => i.Type == IncludeType.Location).ToImmutableList() ??
                   ImmutableList<PlanningCenterAPIClient.Models.CheckInResult.Included>.Empty);
            
            
            var persistedLocations = await _updateRepository.GetPersistedLocations().ConfigureAwait(continueOnCapturedContext: false);
            var newLocations = locations.Where(predicate: l => IsNewLocation(persistedLocations: persistedLocations, location: l)).ToImmutableList();

            if (newLocations.Count == 0)
            {
                return;
            }
            
            await _updateRepository.UpdateLocations(locationUpdates: newLocations.Select(selector: l => MapLocationUpdate(
                    location: l, 
                    attendees: checkIns.Where(predicate: c => c.Attendees != null).SelectMany(selector: c => c.Attendees!).ToImmutableList()))
                .ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);
            
            
            await _updateRepository.EnableUnknownLocationGroup().ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task AutoCheckInOutVolunteers()
        {
            await _updateRepository.AutoCheckInVolunteers().ConfigureAwait(continueOnCapturedContext: false);
            await _updateRepository.AutoCheckoutEveryoneByEndOfDay().ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task UpdatePeople()
        {
            var peopleIds = await _updateRepository.GetCurrentPeopleIds(daysLookBack: DaysLookBack).ConfigureAwait(continueOnCapturedContext: false);

            if (peopleIds.Count == 0)
            {
                return;
            }

            var peopleUpdates = await _planningCenterClient.GetPeopleUpdates(peopleIds: peopleIds).ConfigureAwait(continueOnCapturedContext: false);
            var peoples = MapToPeoples(peopleUpdates: peopleUpdates);
            await _updateRepository.UpdatePersons(peoples: peoples).ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task InsertNewPreCheckIns(IImmutableList<CheckInUpdate> preCheckIns)
        {
            var existingCheckInIds = await _updateRepository.GetExistingCheckInIds(
                checkinIds: preCheckIns.Select(selector: i => i.CheckInId).ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);

            var newCheckins = preCheckIns.Where(predicate: p => !existingCheckInIds.Contains(value: p.CheckInId)).ToImmutableList();

            await _updateRepository.InsertPreCheckIns(preCheckIns: newCheckins).ConfigureAwait(continueOnCapturedContext: false);
        }

        private static ImmutableList<PeopleUpdate> MapToPeoples(ImmutableList<Peoples> peopleUpdates)
        {
            var fieldOptions = peopleUpdates.SelectMany(selector: p => p.Included ?? new List<Included>())
                .Where(predicate: i => i.PeopleIncludedType == PeopleIncludedType.FieldDatum)
                .ToImmutableList();
            
            return peopleUpdates.SelectMany(selector: p => p.Data ?? new List<Datum>())
                .Select(selector: d => MapPeople(people: d, fieldOptions: fieldOptions))
                .ToImmutableList();
        }
        
        private static PeopleUpdate MapPeople(Datum people, ImmutableList<Included> fieldOptions)
        {
            var fieldDataIds = people.Relationships?.FieldData?.Data?.Select(selector: d => d.Id).ToImmutableList() ?? ImmutableList<long>.Empty;
            var personalFieldOptions = fieldOptions.Where(predicate: o => fieldDataIds.Contains(value: o.Id)).ToImmutableList();

            var mayLeaveAloneField = personalFieldOptions.SingleOrDefault(
                predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == MayLeaveAloneFieldId);
            var hasPeopleWithoutPickupPermissionField = personalFieldOptions.SingleOrDefault(
                predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == HasPeopleWithoutPickupPermissionFieldId);

            return MapPeopleUpdate(
                peopleId: people.Id,
                firstName: people.Attributes?.FirstName,
                lastName: people.Attributes?.LastName,
                mayLeaveAlone: !ParseCustomBooleanField(field: mayLeaveAloneField, defaultValue: false),
                hasPeopleWithoutPickupPermission: ParseCustomBooleanField(field: hasPeopleWithoutPickupPermissionField, defaultValue: false));
        }

        private static bool ParseCustomBooleanField(Included? field, bool defaultValue)
        {
            return field?.Attributes?.Value switch
            {
                "true" => true,
                "false" => false,
                _ => defaultValue
            };
        }

        private async Task<IImmutableList<CheckInUpdate>> FilterAndMapToPreCheckIns(ImmutableList<CheckIns> checkIns)
        {
            var persistedLocations = await _updateRepository.GetPersistedLocations().ConfigureAwait(continueOnCapturedContext: false);
            var locationIdsByCheckInsLocationId =
                persistedLocations.ToImmutableDictionary(keySelector: k => k.CheckInsLocationId, elementSelector: v => v.LocationId);
            
            var attendees = checkIns.Where(predicate: c => c.Attendees != null)
                .SelectMany(selector: c => c.Attendees!)
                .Select(selector: a => MapPreCheckIn(
                    attendee: a, 
                    locationIdsByCheckInsLocationId: locationIdsByCheckInsLocationId))
                .ToImmutableList();

            return attendees;
        }
        
        private static CheckInUpdate MapPreCheckIn(Attendee attendee, ImmutableDictionary<long, int> locationIdsByCheckInsLocationId)
        {
            var attributes = attendee.Attributes;
            var checkInsLocationId = attendee.Relationships?.Locations?.Data?.SingleOrDefault()?.Id;
            var peopleId = attendee.Relationships?.Person?.Data?.Id;

            var people = MapPeopleUpdate(
                peopleId: peopleId,
                firstName: attributes?.FirstName,
                lastName: attributes?.LastName);

            var locationId = checkInsLocationId.HasValue && locationIdsByCheckInsLocationId.ContainsKey(key: checkInsLocationId.Value) 
                ? locationIdsByCheckInsLocationId[key: checkInsLocationId.Value] 
                : 30;
                
            return new CheckInUpdate(
                checkInId: attendee.Id,
                peopleId: peopleId,
                attendeeType: attributes?.Kind ?? AttendeeType.Regular,
                securityCode: attributes?.SecurityCode ?? string.Empty,
                locationId: locationId,
                creationDate: attributes?.CreatedAt ?? DateTime.UtcNow,
                person: people);
        }

        private static PeopleUpdate MapPeopleUpdate(
            long? peopleId, 
            string? firstName,
            string? lastName,
            bool mayLeaveAlone = true,
            bool hasPeopleWithoutPickupPermission = false)
        {
            return new(
                peopleId: peopleId,
                firstName: firstName ?? string.Empty,
                lastName: lastName ?? string.Empty,
                mayLeaveAlone: mayLeaveAlone,
                hasPeopleWithoutPickupPermission: hasPeopleWithoutPickupPermission);
        }

        private static LocationUpdate MapLocationUpdate(PlanningCenterAPIClient.Models.CheckInResult.Included location, ImmutableList<Attendee> attendees)
        {
            var attendee = attendees.FirstOrDefault(predicate: a => a.Relationships?.Locations?.Data?.SingleOrDefault()?.Id == location.Id);
            return new LocationUpdate(
                checkInsLocationId: location.Id, 
                name: location.Attributes?.Name ?? string.Empty, 
                eventId: attendee?.Relationships?.Event?.Data?.Id ?? 0);
        }

        private static bool IsNewLocation(ImmutableList<PersistedLocation> persistedLocations, PlanningCenterAPIClient.Models.CheckInResult.Included location)
        {
            return !persistedLocations.Select(selector: p => p.CheckInsLocationId).Contains(value: location.Id);
        }
    }
}
