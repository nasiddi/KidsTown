using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Models;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using Peoples = KidsTown.PlanningCenterApiClient.Models.PeopleResult.People;
using Included = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Included;

namespace KidsTown.BackgroundTasks.PlanningCenter
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

        public async Task<int> FetchDataFromPlanningCenter()
        {
            var checkIns = await _planningCenterClient.GetCheckedInPeople(daysLookBack: DaysLookBack).ConfigureAwait(continueOnCapturedContext: false);
            await UpdateLocations(checkIns: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            var preCheckIns = await FilterAndMapToPreCheckIns(checkIns: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            await InsertNewPreCheckIns(preCheckIns: preCheckIns).ConfigureAwait(continueOnCapturedContext: false);
            
            var families = await UpdatePeople().ConfigureAwait(continueOnCapturedContext: false);
            await UpdateParents(families: families).ConfigureAwait(continueOnCapturedContext: false);

            await AutoCheckInOutVolunteers().ConfigureAwait(continueOnCapturedContext: false);
            await _updateRepository.AutoCheckoutEveryoneByEndOfDay().ConfigureAwait(continueOnCapturedContext: false);

            return preCheckIns.Count;
        }
        
        public void LogTaskRun(bool success, int updateCount, string environment)
        {
            _updateRepository.LogTaskRun(success: success, updateCount: updateCount, environment: environment);
        }

        private async Task UpdateParents(ImmutableList<Family> families)
        {
            var households = new List<Family>();
            foreach (var family in families)
            {
                 var household = await _planningCenterClient.GetHousehold(householdId: family.HouseholdId).ConfigureAwait(continueOnCapturedContext: false);
                 if (household == null)
                 {
                     continue;
                 }
                 
                 households.Add(item: MapHousehold(household: household, family: family));
            }

            var peopleIds = households.SelectMany(selector: h => h.PeopleIds).ToImmutableList();
            var parents = await _planningCenterClient.GetPeopleUpdates(peopleIds: peopleIds);
            var parentUpdates = MapParents(parents: parents, families: households.ToImmutableList());
            await _updateRepository.UpdateParents(parentUpdates: parentUpdates);

        }

        private static Family MapHousehold(Household household, Family family)
        {
            var adults = household.Included?.Where(predicate: i => i.Attributes?.Child == false).ToImmutableList() 
                         ?? ImmutableList<PlanningCenterApiClient.Models.HouseholdResult.Included>.Empty;
            return new Family(familyId: family.FamilyId, householdId: family.HouseholdId, peopleIds: adults.Select(selector: a => a.Id).ToImmutableList());
        }

        private static IImmutableList<ParentUpdate> MapParents(ImmutableList<People> parents, ImmutableList<Family> families)
        {
            var phoneNumbers = parents.SelectMany(selector: p => p.Included ?? new List<Included>())
                .Where(predicate: i => i.PeopleIncludedType == PeopleIncludedType.PhoneNumber)
                .ToImmutableList();
            
            return parents.SelectMany(selector: p => p.Data ?? new List<Datum>())
                .Select(selector: d => MapParent(adult: d, families: families, phoneNumbers: phoneNumbers))
                .ToImmutableList();
        }

        private static ParentUpdate MapParent(
            Datum adult,
            ImmutableList<Family> families,
            ImmutableList<Included> phoneNumbers
        )
        {
            var family = families.First(predicate: f => f.PeopleIds.Contains(value: adult.Id));
            var phoneNumberIds = adult.Relationships?.PhoneNumbers?.Data?.Select(selector: d => d.Id).ToImmutableList() 
                                 ?? ImmutableList<long>.Empty;
            
            var personalNumbers = phoneNumbers.Where(predicate: p => phoneNumberIds.Contains(value: p.Id)).ToImmutableList();

            var number = SelectNumber(numbers: personalNumbers);

            return new ParentUpdate(
                peopleId: adult.Id,
                familyId: family.FamilyId,
                firstName: adult.Attributes?.FirstName ?? string.Empty,
                lastName: adult.Attributes?.LastName ?? string.Empty,
                phoneNumber: number ?? string.Empty);
        }

        private static string? SelectNumber(ImmutableList<Included> numbers)
        {
            switch (numbers.Count)
            {
                case < 1:
                    return null;
                case 1:
                    return numbers.Single().Attributes?.Number;
            }

            var mobileNumbers = numbers.Where(predicate: n => n.Attributes?.NumberType == "Mobile").ToImmutableList();

            var primaryNumber = numbers.FirstOrDefault(predicate: n => n.Attributes?.Primary == true)?.Attributes?.Number;
            if (numbers.Count <= mobileNumbers.Count)
            {
                return primaryNumber;
            }
            
            var mobileNumber = SelectNumber(numbers: mobileNumbers);
            return mobileNumber ?? primaryNumber;
        }

        private async Task UpdateLocations(ImmutableList<CheckIns> checkIns)
        {
            var locations = checkIns.SelectMany(selector: c
                => c.Included?.Where(predicate: i => i.Type == IncludeType.Location).ToImmutableList() ??
                   ImmutableList<PlanningCenterApiClient.Models.CheckInsResult.Included>.Empty);
            
            
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

        private async Task<ImmutableList<Family>> UpdatePeople()
        {
            var peopleIds = await _updateRepository.GetCurrentPeopleIds(daysLookBack: DaysLookBack).ConfigureAwait(continueOnCapturedContext: false);

            if (peopleIds.Count == 0)
            {
                return ImmutableList<Family>.Empty;
            }

            var people = await _planningCenterClient.GetPeopleUpdates(peopleIds: peopleIds).ConfigureAwait(continueOnCapturedContext: false);
            var peopleUpdates = MapPeopleUpdates(peopleUpdates: people);

            var householdIds = peopleUpdates.Where(predicate: p => p.HouseholdId.HasValue)
                .Select(selector: p => p.HouseholdId!.Value).Distinct().ToImmutableList();

            var existingFamilies = await _updateRepository.GetExistingFamilies(householdIds: householdIds);
            var newHouseholdIds = householdIds.Where(predicate: h => existingFamilies.All(predicate: f => f.HouseholdId != h)).ToImmutableList();
            var newFamilies = await _updateRepository.InsertFamilies(newHouseholdIds: newHouseholdIds, peoples: peopleUpdates);

            var families = existingFamilies.Union(second: newFamilies).ToImmutableList();
            
            await _updateRepository.UpdateKids(kids: peopleUpdates, immutableList: families).ConfigureAwait(continueOnCapturedContext: false);

            return families;
        }

        private async Task InsertNewPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns)
        {
            var existingChecksInIds = await _updateRepository.GetExistingCheckInsIds(
                checkinsIds: preCheckIns.Select(selector: i => i.CheckInsId).ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);

            var newCheckins = preCheckIns.Where(predicate: p => !existingChecksInIds.Contains(value: p.CheckInsId)).ToImmutableList();

            await _updateRepository.InsertPreCheckIns(preCheckIns: newCheckins).ConfigureAwait(continueOnCapturedContext: false);
        }

        private static ImmutableList<PeopleUpdate> MapPeopleUpdates(ImmutableList<Peoples> peopleUpdates)
        {
            var fieldOptions = peopleUpdates.SelectMany(selector: p => p.Included ?? new List<Included>())
                .Where(predicate: i => i.PeopleIncludedType == PeopleIncludedType.FieldDatum)
                .ToImmutableList();
            
            var households = peopleUpdates.SelectMany(selector: p => p.Included ?? new List<Included>())
                .Where(predicate: i => i.PeopleIncludedType == PeopleIncludedType.Household)
                .ToImmutableList();
            
            return peopleUpdates.SelectMany(selector: p => p.Data ?? new List<Datum>())
                .Select(selector: d => MapPeople(people: d, fieldOptions: fieldOptions, households: households))
                .ToImmutableList();
        }
        
        private static PeopleUpdate MapPeople(
            Datum people,
            ImmutableList<Included> fieldOptions,
            ImmutableList<Included> households
        )
        {
            var fieldDataIds = people.Relationships?.FieldData?.Data?.Select(selector: d => d.Id).ToImmutableList() ?? ImmutableList<long>.Empty;
            var personalFieldOptions = fieldOptions.Where(predicate: o => fieldDataIds.Contains(value: o.Id)).ToImmutableList();

            var mayLeaveAloneField = personalFieldOptions.SingleOrDefault(
                predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == MayLeaveAloneFieldId);
            var hasPeopleWithoutPickupPermissionField = personalFieldOptions.SingleOrDefault(
                predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == HasPeopleWithoutPickupPermissionFieldId);

            var householdId = people.Relationships?.Households?.Data?.FirstOrDefault()?.Id;
            var household = households.FirstOrDefault(predicate: h => h.Id == householdId);

            return MapPeopleUpdate(
                peopleId: people.Id,
                firstName: people.Attributes?.FirstName,
                lastName: people.Attributes?.LastName,
                householdId: householdId,
                householdName: household?.Attributes?.Name,
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

        private async Task<IImmutableList<CheckInsUpdate>> FilterAndMapToPreCheckIns(ImmutableList<CheckIns> checkIns)
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
        
        private static CheckInsUpdate MapPreCheckIn(Attendee attendee, ImmutableDictionary<long, int> locationIdsByCheckInsLocationId)
        {
            var attributes = attendee.Attributes;
            var checkInsLocationId = attendee.Relationships?.Locations?.Data?.FirstOrDefault()?.Id;
            var peopleId = attendee.Relationships?.Person?.Data?.Id;

            var people = MapPeopleUpdate(
                peopleId: peopleId,
                firstName: attributes?.FirstName,
                lastName: attributes?.LastName);

            var locationId = checkInsLocationId.HasValue && locationIdsByCheckInsLocationId.ContainsKey(key: checkInsLocationId.Value) 
                ? locationIdsByCheckInsLocationId[key: checkInsLocationId.Value] 
                : 30;
                
            return new CheckInsUpdate(
                checkInsId: attendee.Id,
                peopleId: peopleId,
                attendeeType: attributes?.Kind ?? AttendeeType.Regular,
                securityCode: attributes?.SecurityCode ?? string.Empty,
                locationId: locationId,
                creationDate: attributes?.CreatedAt ?? DateTime.UtcNow,
                kid: people);
        }

        private static PeopleUpdate MapPeopleUpdate(
            long? peopleId,
            string? firstName,
            string? lastName,
            long? householdId = null,
            string? householdName = null,
            bool mayLeaveAlone = true,
            bool hasPeopleWithoutPickupPermission = false)
        {
            return new(
                peopleId: peopleId,
                householdId: householdId,
                firstName: firstName ?? string.Empty,
                lastName: lastName ?? string.Empty,
                householdName: householdName,
                mayLeaveAlone: mayLeaveAlone,
                hasPeopleWithoutPickupPermission: hasPeopleWithoutPickupPermission);
        }

        private static LocationUpdate MapLocationUpdate(PlanningCenterApiClient.Models.CheckInsResult.Included location, ImmutableList<Attendee> attendees)
        {
            var attendee = attendees.FirstOrDefault(predicate: a => a.Relationships?.Locations?.Data?.SingleOrDefault()?.Id == location.Id);
            return new LocationUpdate(
                checkInsLocationId: location.Id, 
                name: location.Attributes?.Name ?? string.Empty, 
                eventId: attendee?.Relationships?.Event?.Data?.Id ?? 0);
        }

        private static bool IsNewLocation(ImmutableList<PersistedLocation> persistedLocations, PlanningCenterApiClient.Models.CheckInsResult.Included location)
        {
            return !persistedLocations.Select(selector: p => p.CheckInsLocationId).Contains(value: location.Id);
        }
    }
}
