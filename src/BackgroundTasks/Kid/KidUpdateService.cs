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
            .ConfigureAwait(continueOnCapturedContext: false);
            
        if (kidsPeopleIds.Count == 0)
        {
            return 0;
        }

        var kidsUpdate = await FetchKids(kidsPeopleIds: kidsPeopleIds).ConfigureAwait(continueOnCapturedContext: false);
        var families = await GetNewAnPersistedFamilies(kidsUpdate: kidsUpdate).ConfigureAwait(continueOnCapturedContext: false);

        return await _kidUpdateRepository.UpdateKids(kids: kidsUpdate, families: families)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<ImmutableList<Family>> GetNewAnPersistedFamilies(IImmutableList<PeopleUpdate> kidsUpdate)
    {
        var householdIds = kidsUpdate.Where(predicate: p => p.HouseholdId.HasValue)
            .Select(selector: p => p.HouseholdId!.Value).Distinct().ToImmutableList();

        var existingFamilies = await _kidUpdateRepository.GetExistingFamilies(householdIds: householdIds);
        var newHouseholdIds = householdIds
            .Where(predicate: h => existingFamilies.All(predicate: f => f.HouseholdId != h)).ToImmutableList();
        var newFamilies =
            await _kidUpdateRepository.InsertFamilies(newHouseholdIds: newHouseholdIds, peoples: kidsUpdate);

        var families = existingFamilies.Union(second: newFamilies).ToImmutableList();
        return families;
    }

    private async Task<IImmutableList<PeopleUpdate>> FetchKids(ImmutableList<long> kidsPeopleIds)
    {
            
        var kids = await _planningCenterClient.GetPeopleUpdates(peopleIds: kidsPeopleIds)
            .ConfigureAwait(continueOnCapturedContext: false);
        var kidsUpdate = MapKidsUpdates(peopleUpdates: kids);
        return kidsUpdate;
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
            .Select(selector: d => MapPeopleUpdate(people: d, fieldOptions: fieldOptions, households: households))
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
        var household = households.FirstOrDefault(predicate: h => h.Id == householdId);

        return new(
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
        var fieldDataIds = people.Relationships?.FieldData?.Data?.Select(selector: d => d.Id).ToImmutableList() ??
            ImmutableList<long>.Empty;
        var personalFieldOptions =
            fieldOptions.Where(predicate: o => fieldDataIds.Contains(value: o.Id)).ToImmutableList();
        return personalFieldOptions;
    }

    private static bool ParseCustomBooleanField(
        ImmutableList<Included> fieldOptions,
        bool fallback,
        PeopleFieldId fieldId
    )
    {
        var field = fieldOptions.SingleOrDefault(
            predicate: o => o.Relationships?.FieldDefinition?.Data?.Id == (long?) fieldId);
            
        return field?.Attributes?.Value switch
        {
            "true" => true,
            "false" => false,
            _ => fallback
        };
    }
}