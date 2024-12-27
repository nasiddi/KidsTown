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
        string lastName) => new(
        PeopleId: peopleId,
        HouseholdId: null,
        FirstName: firstName,
        LastName: lastName,
        HouseholdName: null,
        MayLeaveAlone: true,
        HasPeopleWithoutPickupPermission: false);
}