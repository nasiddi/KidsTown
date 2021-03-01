namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class Location
    {
        public readonly int Id;
        public readonly string Name;

        public Location(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}