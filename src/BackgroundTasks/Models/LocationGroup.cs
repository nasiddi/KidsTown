namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class LocationGroup
    {
        public readonly int Id;
        public readonly string Name;

        public LocationGroup(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}