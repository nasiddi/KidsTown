using System.Collections.Immutable;

namespace KidsTown.BackgroundTasks.Adult
{
    public class Family
    {
        public readonly int FamilyId;
        public readonly long HouseholdId;
        public readonly IImmutableList<long> PeopleIds = ImmutableList<long>.Empty;

        public Family(int familyId, long householdId)
        {
            FamilyId = familyId;
            HouseholdId = householdId;
        }
        
        public Family(int familyId, long householdId, IImmutableList<long> peopleIds)
        {
            FamilyId = familyId;
            HouseholdId = householdId;
            PeopleIds = peopleIds;
        }
    }
}