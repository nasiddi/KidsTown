namespace KidsTown.BackgroundTasks.Adult;

public class VolunteerUpdate
{
    public readonly long PeopleId;
    public readonly string FirstName;
    public readonly string LastName;

    public VolunteerUpdate(long peopleId, string firstName, string lastName)
    {
        PeopleId = peopleId;
        FirstName = firstName;
        LastName = lastName;
    }
}