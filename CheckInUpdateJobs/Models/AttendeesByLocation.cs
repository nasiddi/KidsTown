using System.Collections.Immutable;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class AttendeesByLocation
    {
        public string Location { get; init; }
        public ImmutableList<Attendee> Kids { get; init; } = ImmutableList<Attendee>.Empty;
        public ImmutableList<Attendee> Volunteers { get; init; } = ImmutableList<Attendee>.Empty;
        public int LocationGroupId { get; set; }
    }
}