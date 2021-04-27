using System.Collections.Immutable;

namespace KidsTown.BackgroundTasks.Models
{
    public class Family
    {
        public readonly int FamilyId;
        public readonly long HouseholdId;
        public readonly ImmutableList<long> PeopleIds = ImmutableList<long>.Empty;

        public Family(int familyId, long householdId)
        {
            FamilyId = familyId;
            HouseholdId = householdId;
        }
        
        public Family(int familyId, long householdId, ImmutableList<long> peopleIds)
        {
            FamilyId = familyId;
            HouseholdId = householdId;
            PeopleIds = peopleIds;
        }
    }
}