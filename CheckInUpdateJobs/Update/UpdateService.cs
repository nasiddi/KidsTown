using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;
using Peoples = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.People;
using CheckInIncludedType = CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult.TypeEnum;
using Included = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class UpdateService : IUpdateService
    {
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IUpdateRepository _updateRepository;

        private const long MayLeaveAloneFieldId = 438360;
        private const long HasPeopleWithoutPickupPermissionFieldId = 440611;

        public UpdateService(IPlanningCenterClient planningCenterClient, IUpdateRepository updateRepository)
        {
            _planningCenterClient = planningCenterClient;
            _updateRepository = updateRepository;
        }

        public async Task FetchDataFromPlanningCenter()
        {
            var checkIns = await _planningCenterClient.GetCheckedInPeople();
            var preCheckIns = MapToPreCheckIns(checkIns);

            var existingCheckInIds = await _updateRepository.GetExistingCheckInIds(
                    preCheckIns.Select(i => i.CheckInId).ToImmutableList());

            var newCheckins = preCheckIns.Where(p => !existingCheckInIds.Contains(p.CheckInId)).ToImmutableList();

            await _updateRepository.InsertPreCheckIns(newCheckins);

            var peopleIdsPreCheckedInToday = await _updateRepository.GetPeopleIdsPreCheckedInToday();

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
                    mayLeaveAlone: ParseMayLeaveAlone(mayLeaveAloneField),
                    hasPeopleWithoutPickupPermission: ParseHasPeopleWithoutPickupPermission(hasPeopleWithoutPickupPermissionField));
            }
        }

        private static bool ParseHasPeopleWithoutPickupPermission(Included? field)
        {
            return field?.Attributes.Value switch
            {
                "!" => true,
                _ => false
            };
        }
        
        private static bool? ParseMayLeaveAlone(Included? field)
        {
            return field?.Attributes.Value switch
            {
                "true" => true,
                "false" => false,
                _ => null
            };
        }

        private static IImmutableList<CheckInUpdate> MapToPreCheckIns(CheckIns checkIns)
        {
            var locations = checkIns.Included.Where(i => i.Type == CheckInIncludedType.Location).ToImmutableList();
            var locationsByIds = locations.ToImmutableDictionary(k => k.Id, v => v.Attributes.Name);
            
            var attendees = checkIns.Attendees.Select(MapPreCheckIn).ToImmutableList();

            return attendees;
            
            CheckInUpdate MapPreCheckIn(Attendee attendee)
            {
                var attributes = attendee.Attributes;
                var locationId = attendee.Relationships.Locations.Data.Single().Id;
                var peopleId = attendee.Relationships.Person.Data?.Id;

                var people = new PeopleUpdate(
                    peopleId: peopleId,
                    firstName: attributes.FirstName,
                    lastName: attributes.LastName);
                
                return new CheckInUpdate(
                    checkInId: attendee.Id,
                    peopleId: peopleId,
                    attendeeType: attributes.Kind,
                    securityCode: attributes.SecurityCode,
                    location: locationsByIds[locationId],
                    creationDate: attributes.CreatedAt,
                    person: people);
            }
        }
    }
}
