using System.Collections.Immutable;

namespace KidsTown.BackgroundTasks.Adult;

public class Family
{
    public readonly int FamilyId;
    public readonly long HouseholdId;
    public readonly IImmutableList<Person> Members = ImmutableList<Person>.Empty;

    public Family(int familyId, long householdId)
    {
        FamilyId = familyId;
        HouseholdId = householdId;
    }

    public Family(int familyId, long householdId, IImmutableList<Person> members)
    {
        FamilyId = familyId;
        HouseholdId = householdId;
        Members = members;
    }
}