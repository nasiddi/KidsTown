using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;

namespace KidsTown.BackgroundTasks.Adult
{
    public class AdultUpdateService : IAdultUpdateService
    {
        private readonly IAdultUpdateRepository _adultUpdateRepository;
        private readonly IPlanningCenterClient _planningCenterClient;

        public AdultUpdateService(
            IAdultUpdateRepository adultUpdateRepository,
            IPlanningCenterClient planningCenterClient)
        {
            _adultUpdateRepository = adultUpdateRepository;
            _planningCenterClient = planningCenterClient;
        }
        public async Task<int> UpdateParents(int daysLookBack, int batchSize)
        {
            var families = await _adultUpdateRepository.GetFamiliesToUpdate(daysLookBack: daysLookBack, take: batchSize);
            
            var households = await GetHouseholds(families: families).ConfigureAwait(continueOnCapturedContext: false);

            var adultPeopleIds = households.SelectMany(
                selector: h => h.Members.Where(predicate: m => m.IsChild != null && !m.IsChild.Value)
                    .Select(selector: m => m.PeopleId)).ToImmutableList();
            var parents = await _planningCenterClient.GetPeopleUpdates(peopleIds: adultPeopleIds);
            var parentUpdates = MapParents(parents: parents, families: households.ToImmutableList());

            var peopleToRemove = families.SelectMany(selector: f => f.Members.Select(selector: m => m.PeopleId))
                .Except(second: households.SelectMany(selector: h => h.Members.Select(selector: m => m.PeopleId)))
                .ToImmutableList();

            var adultUpdateCount = await _adultUpdateRepository.UpdateAdults(parentUpdates: parentUpdates);
            var removeCount = await _adultUpdateRepository.RemovePeopleFromFamilies(peopleIds: peopleToRemove);

            var updateDateCount = await _adultUpdateRepository.SetFamilyUpdateDate(families: families);
            
            return adultUpdateCount + removeCount + updateDateCount;
        }

        private async Task<List<Family>> GetHouseholds(IImmutableList<Family> families)
        {
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

            return households;
        }

        private static Family MapHousehold(Household household, Family family)
        {
            var members = household.Included?
                              .Select(selector: i => new Person(peopleId: i.Id, isChild: i.Attributes?.Child))
                              .ToImmutableList()
                         ?? ImmutableList<Person>.Empty;
            
            return new Family(familyId: family.FamilyId, householdId: family.HouseholdId,
                members: members);
        }
        
        private static IImmutableList<AdultUpdate> MapParents(
            IImmutableList<People> parents,
            IImmutableList<Family> families
        )
        {
            var phoneNumbers = parents.SelectMany(selector: p => p.Included ?? new List<PlanningCenterApiClient.Models.PeopleResult.Included>())
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
            IImmutableList<PlanningCenterApiClient.Models.PeopleResult.Included> phoneNumbers
        )
        {
            var family = families.FirstOrDefault(predicate: f => f.Members.Select(selector: m => m.PeopleId)
                .Contains(value: adult.Id));
            var phoneNumberIds = adult.Relationships?.PhoneNumbers?.Data?.Select(selector: d => d.Id).ToImmutableList()
                                 ?? ImmutableList<long>.Empty;

            var personalNumbers = phoneNumbers.Where(predicate: p => phoneNumberIds.Contains(value: p.Id))
                .ToImmutableList();

            var number = SelectNumber(numbers: personalNumbers);

            if (family == null)
            {
                return null;
            }

            return new AdultUpdate(
                peopleId: adult.Id,
                familyId: family.FamilyId,
                phoneNumberId: number?.Id,
                firstName: adult.Attributes?.FirstName ?? string.Empty,
                lastName: adult.Attributes?.LastName ?? string.Empty,
                phoneNumber: number?.Number);
        }
        
        private static PhoneNumber? SelectNumber(
            IImmutableList<PlanningCenterApiClient.Models.PeopleResult.Included> numbers)
        {
            switch (numbers.Count)
            {
                case < 1:
                    return null;
                case 1:
                    var included = numbers.Single();
                    return included.Attributes?.Number != null ? new PhoneNumber(id: included.Id, number: included.Attributes.Number) : null;
                case > 1:
                    var mobileNumbers = numbers.Where(predicate: n => n.Attributes?.NumberType == "Mobile").ToImmutableList();
                    var primaryContact = numbers.FirstOrDefault(predicate: n => n.Attributes?.Primary == true);
                    var primaryNumber = primaryContact?.Attributes?.Number != null ? new PhoneNumber(id: primaryContact.Id, number: primaryContact.Attributes.Number) : null;
                    
                    if (numbers.Count <= mobileNumbers.Count)
                    {
                        return primaryNumber;
                    }

                    var mobileNumber = SelectNumber(numbers: mobileNumbers);
                    return mobileNumber ?? primaryNumber;
            }
        }

        private class PhoneNumber
        {
            public readonly long Id;
            public readonly string Number;
            public PhoneNumber(long id, string number)
            {
                Id = id;
                Number = number;
            }
        }
    }
}