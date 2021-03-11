namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class PersistedLocation
    {
        public int LocationId;
        public long CheckInsLocationId;

        public PersistedLocation(int locationId, long checkInsLocationId)
        {
            LocationId = locationId;
            CheckInsLocationId = checkInsLocationId;
        }
    }
}