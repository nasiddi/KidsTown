using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.KidsTown.Models
{
    public class AttendeesByLocation
    {
        public string Location { get; init; } = string.Empty;
        public ImmutableList<Attendee> Kids { get; init; } = ImmutableList<Attendee>.Empty;
        public ImmutableList<Attendee> Volunteers { get; init; } = ImmutableList<Attendee>.Empty;
        public int LocationGroupId { get; init; }
    }
}