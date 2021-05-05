namespace KidsTown.BackgroundTasks.Attendance
{
    public class PersistedLocation
    {
        public readonly int LocationId;
        public readonly long CheckInsLocationId;

        public PersistedLocation(int locationId, long checkInsLocationId)
        {
            LocationId = locationId;
            CheckInsLocationId = checkInsLocationId;
        }
    }
}