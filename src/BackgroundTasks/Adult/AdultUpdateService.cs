using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using Included = KidsTown.PlanningCenterApiClient.Models.HouseholdResult.Included;

namespace KidsTown.BackgroundTasks.Adult
{
    public class AdultUpdateService : IAdultUpdateService
    {
        private readonly IAdultUpdateRepository _adultUpdateRepository;
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IUpdateRepository _updateRepository;

        public AdultUpdateService(
            IAdultUpdateRepository adultUpdateRepository,
            IPlanningCenterClient planningCenterClient, 
            IUpdateRepository updateRepository)
        {
            _adultUpdateRepository = adultUpdateRepository;
            _planningCenterClient = planningCenterClient;
            _updateRepository = updateRepository;
        }
        public async Task<int> UpdateParents(int daysLookBack, int batchSize)
        {
            var families = await _adultUpdateRepository.GetFamiliesToUpdate(daysLookBack: daysLookBack, take: batchSize);
            
            var households = new List<Family>();
            
            foreach (var family in families)
            {
                var household = await _planningCenterClient.GetHousehold(householdId: family.HouseholdId)
                    .ConfigureAwait(continueOnCapturedContext: false);
                if (household == null)
                {
                    continue;
                }

                households.Add(item: MapHousehold(household: household, family: family));
            }

            var peopleIds = households.SelectMany(selector: h => h.PeopleIds).ToImmutableList();
            var parents = await _planningCenterClient.GetPeopleUpdates(peopleIds: peopleIds);
            var parentUpdates = MapParents(parents: parents, families: households.ToImmutableList());
            return await _updateRepository.UpdateParents(parentUpdates: parentUpdates);
        }
        
        private static Family MapHousehold(Household household, Family family)
        {
            var adults = household.Included?.Where(predicate: i => i.Attributes?.Child == false).ToImmutableList()
                         ?? ImmutableList<Included>.Empty;
            return new Family(familyId: family.FamilyId, householdId: family.HouseholdId,
                peopleIds: adults.Select(selector: a => a.Id).ToImmutableList());
        }
        
        private static IImmutableList<AdultUpdate> MapParents(
            IImmutableList<People> parents,
            IImmutableList<Family> families
        )
        {
            var phoneNumbers = parents.SelectMany(selector: p => p.Included ?? new List<KidsTown.PlanningCenterApiClient.Models.PeopleResult.Included>())
                .Where(predicate: i => i.PeopleIncludedType == PeopleIncludedType.PhoneNumber)
                .ToImmutableList();

            var parentUpdates = parents.SelectMany(selector: p => p.Data ?? new List<Datum>())
                .Select(selector: d => MapParent(adult: d, families: families, phoneNumbers: phoneNumbers))
                .ToImmutableList();

            return parentUpdates.Where(predicate: p => p != null).Select(selector: p => p!).ToImmutableList();
        }
        
        private static AdultUpdate? MapParent(
            Datum adult,
            IImmutableList<Family> families,
            IImmutableList<KidsTown.PlanningCenterApiClient.Models.PeopleResult.Included> phoneNumbers
        )
        {
            var family = families.FirstOrDefault(predicate: f => f.PeopleIds.Contains(value: adult.Id));
            var phoneNumberIds = adult.Relationships?.PhoneNumbers?.Data?.Select(selector: d => d.Id).ToImmutableList()
                                 ?? ImmutableList<long>.Empty;

            var personalNumbers = phoneNumbers.Where(predicate: p => phoneNumberIds.Contains(value: p.Id))
                .ToImmutableList();

            var number = SelectNumber(numbers: personalNumbers);

            if (family == null || number == null)
            {
                return null;
            }

            return new AdultUpdate(
                peopleId: adult.Id,
                familyId: family.FamilyId,
                firstName: adult.Attributes?.FirstName ?? string.Empty,
                lastName: adult.Attributes?.LastName ?? string.Empty,
                phoneNumber: number);
        }
        
        private static string? SelectNumber(
            IImmutableList<KidsTown.PlanningCenterApiClient.Models.PeopleResult.Included> numbers)
        {
            switch (numbers.Count)
            {
                case < 1:
                    return null;
                case 1:
                    return numbers.Single().Attributes?.Number;
                case > 1:
                    var mobileNumbers = numbers.Where(predicate: n => n.Attributes?.NumberType == "Mobile").ToImmutableList();

                    var primaryNumber = numbers.FirstOrDefault(predicate: n => n.Attributes?.Primary == true)?.Attributes
                        ?.Number;
                    if (numbers.Count <= mobileNumbers.Count)
                    {
                        return primaryNumber;
                    }

                    var mobileNumber = SelectNumber(numbers: mobileNumbers);
                    return mobileNumber ?? primaryNumber;
            }
        }
    }
}