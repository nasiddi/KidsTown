using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;
using Microsoft.Extensions.Configuration;
using Peoples = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.People;
using Included = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class UpdateService : IUpdateService
    {
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IUpdateRepository _updateRepository;
        private readonly IConfiguration _configuration;

        private const long MayLeaveAloneFieldId = 438360;
        private const long HasPeopleWithoutPickupPermissionFieldId = 441655;

        private const int DaysLookBack = 30;

        public UpdateService(IPlanningCenterClient planningCenterClient, IUpdateRepository updateRepository, IConfiguration configuration)
        {
            _planningCenterClient = planningCenterClient;
            _updateRepository = updateRepository;
            _configuration = configuration;
        }

        public async Task FetchDataFromPlanningCenter()
        {
            var checkIns = await _planningCenterClient.GetCheckedInPeople(DaysLookBack);
            
            var eventIds = _configuration.GetSection("EventIds").Get<long[]>();

            var preCheckIns = FilterAndMapToPreCheckIns(checkIns, eventIds.ToImmutableList());

            var existingCheckInIds = await _updateRepository.GetExistingCheckInIds(
                    preCheckIns.Select(i => i.CheckInId).ToImmutableList());

            var newCheckins = preCheckIns.Where(p => !existingCheckInIds.Contains(p.CheckInId)).ToImmutableList();

            await _updateRepository.InsertPreCheckIns(newCheckins);

            var peopleIdsPreCheckedInToday = await _updateRepository.GetPeopleIdsPreCheckedIns(DaysLookBack);

            if (peopleIdsPreCheckedInToday.Count == 0)
            {
                return;
            }
            
            var peopleUpdates = await _planningCenterClient.GetPeopleUpdates(peopleIdsPreCheckedInToday);
            var peoples = MapToPeoples(peopleUpdates);
            await _updateRepository.UpdatePersons(peoples);
        }

        private static ImmutableList<PeopleUpdate> MapToPeoples(Peoples peopleUpdates)
        {
            var fieldOptions = peopleUpdates.Included.Where(i => i.PeopleIncludedType == PeopleIncludedType.FieldDatum).ToImmutableList();
            return peopleUpdates.Data.Select(MapPeople).ToImmutableList();

            PeopleUpdate MapPeople(Datum people)
            {
                var fieldDataIds = people.Relationships.FieldData.Data.Select(d => d.Id).ToImmutableList();
                var personalFieldOptions = fieldOptions.Where(o => fieldDataIds.Contains(o.Id)).ToImmutableList();

                var mayLeaveAloneField = personalFieldOptions.SingleOrDefault(o => o.Relationships.FieldDefinition.Data.Id == MayLeaveAloneFieldId);
                var hasPeopleWithoutPickupPermissionField = personalFieldOptions.SingleOrDefault(o => o.Relationships.FieldDefinition.Data.Id == HasPeopleWithoutPickupPermissionFieldId);

                return new PeopleUpdate(
                    peopleId: people.Id,
                    firstName: people.Attributes.FirstName,
                    lastName: people.Attributes.LastName,
                    mayLeaveAlone: ParseCustomBooleanField(mayLeaveAloneField, true),
                    hasPeopleWithoutPickupPermission: ParseCustomBooleanField(hasPeopleWithoutPickupPermissionField, false));
            }
        }

        private static bool ParseCustomBooleanField(Included? field, bool defaultValue)
        {
            return field?.Attributes.Value switch
            {
                "true" => true,
                "false" => false,
                _ => defaultValue
            };
        }

        private static IImmutableList<CheckInUpdate> FilterAndMapToPreCheckIns(CheckIns checkIns, ImmutableList<long> eventIds)
        {
            var locations = checkIns.Included.Where(i => i.Type == IncludeType.Location).ToImmutableList();
            var locationsByIds = locations.ToImmutableDictionary(k => k.Id, v => v.Attributes.Name);

            // Todo this can be removed once no more invalid data is pulled
            var checkInsWithLocation = checkIns.Attendees.Where(a => a.Relationships.Locations.Data.Count == 1).ToImmutableList();

            var checkInsFilteredByEvents = checkInsWithLocation.Where(c => eventIds.Contains(c.Relationships.Event.Data.Id)).ToImmutableList();
            var attendees = checkInsFilteredByEvents.Select(MapPreCheckIn).ToImmutableList();

            return attendees;
            
            CheckInUpdate MapPreCheckIn(Attendee attendee)
            {
                var attributes = attendee.Attributes;
                var locationId = attendee.Relationships.Locations.Data.Single().Id;
                var peopleId = attendee.Relationships.Person.Data?.Id;
                var eventId = attendee.Relationships.Event.Data.Id;

                var people = new PeopleUpdate(
                    peopleId: peopleId,
                    firstName: attributes.FirstName,
                    lastName: attributes.LastName);

                return new CheckInUpdate(
                    checkInId: attendee.Id,
                    peopleId: peopleId,
                    eventId: eventId,
                    attendeeType: attributes.Kind,
                    securityCode: attributes.SecurityCode,
                    location: locationsByIds[locationId],
                    creationDate: attributes.CreatedAt,
                    person: people);
            }
        }
    }
}
