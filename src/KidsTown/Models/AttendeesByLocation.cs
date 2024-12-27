using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.KidsTown.Models;

public class AttendeesByLocation
{
    public string Location { get; init; } = string.Empty;

    public IImmutableList<Attendee> Kids { get; init; } = ImmutableList<Attendee>.Empty;

    public IImmutableList<Attendee> Volunteers { get; init; } = ImmutableList<Attendee>.Empty;

    public int LocationGroupId { get; init; }
}