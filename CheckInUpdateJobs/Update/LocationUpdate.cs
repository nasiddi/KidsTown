namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class LocationUpdate
    {
        public long CheckInsLocationId;
        public string Name;
        public long EventId;

        public LocationUpdate(long checkInsLocationId, string name, long eventId)
        {
            CheckInsLocationId = checkInsLocationId;
            Name = name;
            EventId = eventId;
        }
    }
}