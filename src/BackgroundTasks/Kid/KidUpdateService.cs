using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Common;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using KidsTown.Shared;

namespace KidsTown.BackgroundTasks.Kid;

public class KidUpdateService(IPlanningCenterClient planningCenterClient, IKidUpdateRepository kidUpdateRepository)
    : IKidUpdateService
{
    public async Task<int> UpdateKids(int daysLookBack, int batchSize)
    {
        var kidsPeopleIds = await kidUpdateRepository.GetKidsPeopleIdToUpdate(daysLookBack, batchSize)
            .ConfigureAwait(continueOnCapturedContext: false);

        if (kidsPeopleIds.Count == 0)
        {
            return 0;
        }

        var kidsUpdate = await FetchKids(kidsPeopleIds).ConfigureAwait(continueOnCapturedContext: false);
        var families = await GetNewAnPersistedFamilies(kidsUpdate).ConfigureAwait(continueOnCapturedContext: false);

        return await kidUpdateRepository.UpdateKids(kidsUpdate, families)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<ImmutableList<Family>> GetNewAnPersistedFamilies(IImmutableList<PeopleUpdate> kidsUpdate)
    {
        var householdIds = kidsUpdate.Where(p => p.HouseholdId.HasValue)
            .Select(p => p.HouseholdId!.Value)
            .Distinct()
            .ToImmutableList();

        var existingFamilies = await kidUpdateRepository.GetExistingFamilies(householdIds);
        var newHouseholdIds = householdIds
            .Where(h => existingFamilies.All(f => f.HouseholdId != h))
            .ToImmutableList();

        var newFamilies =
            await kidUpdateRepository.InsertFamilies(newHouseholdIds, kidsUpdate);

        var families = existingFamilies.Union(newFamilies).ToImmutableList();
        return families;
    }

    private async Task<IImmutableList<PeopleUpdate>> FetchKids(ImmutableList<long> kidsPeopleIds)
    {
        var kids = await planningCenterClient.GetPeopleUpdates(kidsPeopleIds)
            .ConfigureAwait(continueOnCapturedContext: false);

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
            .Select(d => MapPeopleUpdate(d, fieldOptions, households))
            .ToImmutableList();
    }

    private static PeopleUpdate MapPeopleUpdate(
        Datum people,
        IImmutableList<Included> fieldOptions,
        IImmutableList<Included> households
    )
    {
        var personalFieldOptions = GetFieldOptions(people, fieldOptions);

        var householdId = people.Relationships?.Households?.Data?.FirstOrDefault()?.Id;
        var household = households.FirstOrDefault(h => h.Id == householdId);

        return new PeopleUpdate(
            people.Id,
            FirstName: people.Attributes?.FirstName ?? string.Empty,
            LastName: people.Attributes?.LastName ?? string.Empty,
            HouseholdId: householdId,
            HouseholdName: household?.Attributes?.Name,
            MayLeaveAlone: !ParseCustomBooleanField(
                personalFieldOptions,
                fallback: false,
                PeopleFieldId.NeedsToBePickedUp),
            HasPeopleWithoutPickupPermission: ParseCustomBooleanField(
                personalFieldOptions,
                fallback: false,
                PeopleFieldId.Kab));
    }

    private static ImmutableList<Included> GetFieldOptions(Datum people, IImmutableList<Included> fieldOptions)
    {
        var fieldDataIds = people.Relationships?.FieldData?.Data?.Select(d => d.Id).ToImmutableList() ?? ImmutableList<long>.Empty;
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
        var field = fieldOptions.SingleOrDefault(o => o.Relationships?.FieldDefinition?.Data?.Id == (long?) fieldId);

        return field?.Attributes?.Value switch
        {
            "true" => true,
            "false" => false,
            _ => fallback
        };
    }
}