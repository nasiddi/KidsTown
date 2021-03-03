namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class Attendee
    {
        public string Name { get; init; } = string.Empty;
        public string AttendanceType { get; init; } = string.Empty;
        public int LocationId { get; init; }
        public CheckState CheckState { get; init; }
    }
}