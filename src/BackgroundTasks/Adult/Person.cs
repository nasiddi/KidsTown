namespace KidsTown.BackgroundTasks.Adult;

public class Person
{
    public readonly long PeopleId;
    public readonly bool? IsChild;

    public Person(long peopleId, bool? isChild)
    {
        PeopleId = peopleId;
        IsChild = isChild;
    }
        
    public Person(long peopleId)
    {
        PeopleId = peopleId;
    }
}