using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using Included = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Included;

namespace KidsTown.BackgroundTasks.Adult;

public class AdultUpdateService(
        IAdultUpdateRepository adultUpdateRepository,
        IPlanningCenterClient planningCenterClient)
    : IAdultUpdateService
{
    public async Task<int> UpdateParents(int daysLookBack, int batchSize)
    {
        var families = await adultUpdateRepository.GetFamiliesToUpdate(daysLookBack, batchSize);

        var households = await GetHouseholds(families).ConfigureAwait(continueOnCapturedContext: false);

        var adultPeopleIds = households.SelectMany(
                h => h.Members.Where(m => m.IsChild != null && !m.IsChild.Value)
                    .Select(m => m.PeopleId))
            .ToImmutableList();

        var parents = await planningCenterClient.GetPeopleUpdates(adultPeopleIds);
        var parentUpdates = MapParents(parents, households.ToImmutableList());

        var peopleToRemove = families.SelectMany(f => f.Members.Select(m => m.PeopleId))
            .Except(households.SelectMany(h => h.Members.Select(m => m.PeopleId)))
            .ToImmutableList();

        var adultUpdateCount = await adultUpdateRepository.UpdateAdults(parentUpdates);
        var removeCount = await adultUpdateRepository.RemovePeopleFromFamilies(peopleToRemove);

        var updateDateCount = await adultUpdateRepository.SetFamilyUpdateDate(families);

        return adultUpdateCount + removeCount + updateDateCount;
    }

    public async Task<int> UpdateVolunteersWithoutFamilies(int daysLookBack, int batchSize)
    {
        var peopleIds = await adultUpdateRepository.GetVolunteerPersonIdsWithoutFamiliesToUpdate(daysLookBack, batchSize);
        var people = await planningCenterClient.GetPeopleUpdates(peopleIds);
        var data = people.SelectMany(p => p.Data ?? new List<Datum>()).ToImmutableList();
        var volunteerUpdates = data.Select(MapVolunteerUpdate).ToImmutableList();
        return await adultUpdateRepository.UpdateVolunteers(peopleIds, volunteerUpdates);
    }

    private static VolunteerUpdate MapVolunteerUpdate(Datum datum)
    {
        return new VolunteerUpdate(
            datum.Id,
            datum.Attributes?.FirstName ?? string.Empty,
            datum.Attributes?.LastName ?? string.Empty);
    }

    private async Task<List<Family>> GetHouseholds(IImmutableList<Family> families)
    {
        var households = new List<Family>();

        foreach (var family in families)
        {
            var household = await planningCenterClient.GetHousehold(family.HouseholdId)
                .ConfigureAwait(continueOnCapturedContext: false);

            if (household == null)
            {
                continue;
            }

            households.Add(MapHousehold(household, family));
        }

        return households;
    }

    private static Family MapHousehold(Household household, Family family)
    {
        var members = household.Included?
                .Select(i => new Person(i.Id, i.Attributes?.Child))
                .ToImmutableList()
            ?? ImmutableList<Person>.Empty;

        return new Family(
            family.FamilyId,
            family.HouseholdId,
            members);
    }

    private static IImmutableList<AdultUpdate> MapParents(
        IImmutableList<People> parents,
        IImmutableList<Family> families
    )
    {
        var phoneNumbers = parents.SelectMany(p => p.Included ?? new List<Included>())
            .Where(i => i.PeopleIncludedType == PeopleIncludedType.PhoneNumber)
            .ToImmutableList();

        var parentUpdates = parents.SelectMany(p => p.Data ?? new List<Datum>())
            .Select(d => MapParent(d, families, phoneNumbers))
            .ToImmutableList();

        return parentUpdates.Where(p => p != null).Select(p => p!).ToImmutableList();
    }

    private static AdultUpdate? MapParent(
        Datum adult,
        IImmutableList<Family> families,
        IImmutableList<Included> phoneNumbers
    )
    {
        var family = families.FirstOrDefault(
            f => f.Members.Select(m => m.PeopleId)
                .Contains(adult.Id));

        var phoneNumberIds = adult.Relationships?.PhoneNumbers?.Data?.Select(d => d.Id).ToImmutableList()
            ?? ImmutableList<long>.Empty;

        var personalNumbers = phoneNumbers.Where(p => phoneNumberIds.Contains(p.Id))
            .ToImmutableList();

        var number = SelectNumber(personalNumbers);

        if (family == null)
        {
            return null;
        }

        return new AdultUpdate(
            adult.Id,
            family.FamilyId,
            number?.Id,
            adult.Attributes?.FirstName ?? string.Empty,
            adult.Attributes?.LastName ?? string.Empty,
            number?.Number);
    }

    private static PhoneNumber? SelectNumber(
        IImmutableList<Included> numbers)
    {
        switch (numbers.Count)
        {
            case < 1:
                return null;
            case 1:
                var included = numbers.Single();
                return included.Attributes?.Number != null ? new PhoneNumber(included.Id, included.Attributes.Number) : null;
            case > 1:
                var mobileNumbers = numbers.Where(n => n.Attributes?.NumberType == "Mobile").ToImmutableList();
                var primaryContact = numbers.FirstOrDefault(n => n.Attributes?.Primary == true);
                var primaryNumber = primaryContact?.Attributes?.Number != null ? new PhoneNumber(primaryContact.Id, primaryContact.Attributes.Number) : null;

                if (numbers.Count <= mobileNumbers.Count)
                {
                    return primaryNumber;
                }

                var mobileNumber = SelectNumber(mobileNumbers);
                return mobileNumber ?? primaryNumber;
        }
    }

    private record PhoneNumber(long Id, string Number);
}