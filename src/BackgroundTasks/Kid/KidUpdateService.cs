using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using KidsTown.Shared;

namespace KidsTown.BackgroundTasks.Kid
{
    public class KidUpdateService : IKidUpdateService
    {
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IUpdateRepository _updateRepository;

        public KidUpdateService(IPlanningCenterClient planningCenterClient, IUpdateRepository updateRepository)
        {
            _planningCenterClient = planningCenterClient;
            _updateRepository = updateRepository;
        }
        
        public async Task<int> UpdateKids(int daysLookBack)
        {
            var typedAttendees = await _updateRepository.GetCurrentPeopleIds(daysLookBack: daysLookBack)
                .ConfigureAwait(continueOnCapturedContext: false);

            if (typedAttendees.Count == 0)
            {
                return 0;
            }

            var kidsPeopleIds = typedAttendees
                .Where(predicate: a => a.AttendanceTypeId == AttendanceTypeId.Regular || a.AttendanceTypeId == AttendanceTypeId.Guest)
                .Select(selector: a => a.PeopleId).ToImmutableList();
            var kids = await _planningCenterClient.GetPeopleUpdates(peopleIds: kidsPeopleIds)
                .ConfigureAwait(continueOnCapturedContext: false);
            var kidsUpdate = MapKidsUpdates(peopleUpdates: kids);

            var householdIds = kidsUpdate.Where(predicate: p => p.HouseholdId.HasValue)
                .Select(selector: p => p.HouseholdId!.Value).Distinct().ToImmutableList();

            var existingFamilies = await _updateRepository.GetExistingFamilies(householdIds: householdIds);
            var newHouseholdIds = householdIds
                .Where(predicate: h => existingFamilies.All(predicate: f => f.HouseholdId != h)).ToImmutableList();
            var newFamilies =
                await _updateRepository.InsertFamilies(newHouseholdIds: newHouseholdIds, peoples: kidsUpdate);

            var families = existingFamilies.Union(second: newFamilies).ToImmutableList();

            return await _updateRepository.UpdateKids(kids: kidsUpdate, families: families)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private static IImmutableList<PeopleUpdate> MapKidsUpdates(IImmutableList<People> peopleUpdates)
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
            IImmutableList<Included> fieldOptions,
            IImmutableList<Included> households
        )
        {
            var fieldDataIds = people.Relationships?.FieldData?.Data?.Select(selector: d => d.Id).ToImmutableList() ??
                               ImmutableList<long>.Empty;
            var personalFieldOptions =
                fieldOptions.Where(predicate: o => fieldDataIds.Contains(value: o.Id)).ToImmutableList();

            var mayLeaveAloneField = personalFieldOptions.SingleOrDefault(
                predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == (long?) PeopleFieldId.NeedsToBePickedUp);
            var hasPeopleWithoutPickupPermissionField = personalFieldOptions.SingleOrDefault(
                predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == (long?) PeopleFieldId.Kab);

            var householdId = people.Relationships?.Households?.Data?.FirstOrDefault()?.Id;
            var household = households.FirstOrDefault(predicate: h => h.Id == householdId);

            return MapPeopleUpdate(
                peopleId: people.Id,
                firstName: people.Attributes?.FirstName,
                lastName: people.Attributes?.LastName,
                householdId: householdId,
                householdName: household?.Attributes?.Name,
                mayLeaveAlone: !ParseCustomBooleanField(field: mayLeaveAloneField, defaultValue: false),
                hasPeopleWithoutPickupPermission: ParseCustomBooleanField(field: hasPeopleWithoutPickupPermissionField,
                    defaultValue: false));
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
        
        private static PeopleUpdate MapPeopleUpdate(
            long? peopleId,
            string? firstName,
            string? lastName,
            long? householdId = null,
            string? householdName = null,
            bool mayLeaveAlone = true,
            bool hasPeopleWithoutPickupPermission = false
        )
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
        
        
    }
}