using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Common;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using KidsTown.Shared;

namespace KidsTown.BackgroundTasks.Kid
{
    public class KidUpdateService : IKidUpdateService
    {
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IKidUpdateRepository _kidUpdateRepository;

        public KidUpdateService(IPlanningCenterClient planningCenterClient, IKidUpdateRepository kidUpdateRepository)
        {
            _planningCenterClient = planningCenterClient;
            _kidUpdateRepository = kidUpdateRepository;
        }
        
        public async Task<int> UpdateKids(int daysLookBack, int batchSize)
        {
            var kidsPeopleIds = await _kidUpdateRepository.GetKidsPeopleIdToUpdate(daysLookBack: daysLookBack, take: batchSize)
                .ConfigureAwait(false);
            
            if (kidsPeopleIds.Count == 0)
            {
                return 0;
            }

            var kidsUpdate = await FetchKids(kidsPeopleIds);
            var families = await GetNewAnPersistedFamilies(kidsUpdate);

            return await _kidUpdateRepository.UpdateKids(kids: kidsUpdate, families: families)
                .ConfigureAwait(false);
        }

        private async Task<ImmutableList<Family>> GetNewAnPersistedFamilies(IImmutableList<PeopleUpdate> kidsUpdate)
        {
            var householdIds = kidsUpdate.Where(p => p.HouseholdId.HasValue)
                .Select(p => p.HouseholdId!.Value).Distinct().ToImmutableList();

            var existingFamilies = await _kidUpdateRepository.GetExistingFamilies(householdIds);
            var newHouseholdIds = householdIds
                .Where(h => existingFamilies.All(f => f.HouseholdId != h)).ToImmutableList();
            var newFamilies =
                await _kidUpdateRepository.InsertFamilies(newHouseholdIds: newHouseholdIds, peoples: kidsUpdate);

            var families = existingFamilies.Union(newFamilies).ToImmutableList();
            return families;
        }

        private async Task<IImmutableList<PeopleUpdate>> FetchKids(ImmutableList<long> kidsPeopleIds)
        {
            
            var kids = await _planningCenterClient.GetPeopleUpdates(kidsPeopleIds)
                .ConfigureAwait(false);
            var kidsUpdate = MapKidsUpdates(kids);
            return kidsUpdate;
        }

        private static IImmutableList<PeopleUpdate> MapKidsUpdates(IImmutableList<People> peopleUpdates)
        {
            var fieldOptions = peopleUpdates.SelectMany(p => p.Included ?? new List<Included>())
                .Where(i => i.PeopleIncludedType == PeopleIncludedType.FieldDatum)
                .ToImmutableList();

            var households = peopleUpdates.SelectMany(p => p.Included ?? new List<Included>())
                .Where(i => i.PeopleIncludedType == PeopleIncludedType.Household)
                .ToImmutableList();

            return peopleUpdates.SelectMany(p => p.Data ?? new List<Datum>())
                .Select(d => MapPeopleUpdate(people: d, fieldOptions: fieldOptions, households: households))
                .ToImmutableList();
        }

        private static PeopleUpdate MapPeopleUpdate(
            Datum people,
            IImmutableList<Included> fieldOptions,
            IImmutableList<Included> households
        )
        {
            var personalFieldOptions = GetFieldOptions(people: people, fieldOptions: fieldOptions);

            var householdId = people.Relationships?.Households?.Data?.FirstOrDefault()?.Id;
            var household = households.FirstOrDefault(h => h.Id == householdId);

            return new PeopleUpdate(
                peopleId: people.Id,
                firstName: people.Attributes?.FirstName ?? string.Empty,
                lastName: people.Attributes?.LastName ?? string.Empty,
                householdId: householdId,
                householdName: household?.Attributes?.Name,
                mayLeaveAlone: !ParseCustomBooleanField(
                    fieldOptions: personalFieldOptions,
                    fallback: false,
                    fieldId: PeopleFieldId.NeedsToBePickedUp),
                hasPeopleWithoutPickupPermission: ParseCustomBooleanField(
                    fieldOptions: personalFieldOptions,
                    fallback: false,
                    fieldId: PeopleFieldId.Kab));
        }

        private static ImmutableList<Included> GetFieldOptions(Datum people, IImmutableList<Included> fieldOptions)
        {
            var fieldDataIds = people.Relationships?.FieldData?.Data?.Select(d => d.Id).ToImmutableList() ??
                               ImmutableList<long>.Empty;
            var personalFieldOptions =
                fieldOptions.Where(o => fieldDataIds.Contains(o.Id)).ToImmutableList();
            return personalFieldOptions;
        }

        private static bool ParseCustomBooleanField(
            ImmutableList<Included> fieldOptions,
            bool fallback,
            PeopleFieldId fieldId
        )
        {
            var field = fieldOptions.SingleOrDefault(
                o => o.Relationships?.FieldDefinition?.Data?.Id == (long?) fieldId);
            
            return field?.Attributes?.Value switch
            {
                "true" => true,
                "false" => false,
                _ => fallback
            };
        }
    }
}