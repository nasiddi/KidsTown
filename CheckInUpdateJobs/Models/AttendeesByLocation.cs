using System.Collections.Immutable;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class AttendeesByLocation
    {
        public int LocationId { get; init; }
        public ImmutableList<Attendee> Kids { get; init; } = ImmutableList<Attendee>.Empty;
        public ImmutableList<Attendee> Volunteers { get; init; } = ImmutableList<Attendee>.Empty;
    }
}