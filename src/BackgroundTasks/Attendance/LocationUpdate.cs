namespace KidsTown.BackgroundTasks.Attendance;

public class LocationUpdate
{
    public readonly long CheckInsLocationId;
    public readonly string Name;
    public readonly long EventId;

    public LocationUpdate(long checkInsLocationId, string name, long eventId)
    {
        CheckInsLocationId = checkInsLocationId;
        Name = name;
        EventId = eventId;
    }
}