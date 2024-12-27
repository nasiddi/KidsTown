namespace KidsTown.BackgroundTasks.Common;

public record PeopleUpdate(
    long? PeopleId,
    long? HouseholdId,
    string FirstName,
    string LastName,
    string? HouseholdName,
    bool MayLeaveAlone,
    bool HasPeopleWithoutPickupPermission)
{
    public static PeopleUpdate CreateSimple(
        long? peopleId,
        string firstName,
        string lastName)
    {
        return new PeopleUpdate(
            peopleId,
            HouseholdId: null,
            firstName,
            lastName,
            HouseholdName: null,
            MayLeaveAlone: true,
            HasPeopleWithoutPickupPermission: false);
    }
}