namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class Location
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int LocationGroupId;

        public Location(int id, string name, int locationGroupId)
        {
            Id = id;
            Name = name;
            LocationGroupId = locationGroupId;
        }
    }
}