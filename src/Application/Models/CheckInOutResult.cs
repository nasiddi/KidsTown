using System.Collections.Immutable;
using KidsTown.Shared;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.Application.Models;

public class CheckInOutResult
{
    public string Text { get; init; } = string.Empty;

    public AlertLevel AlertLevel { get; init; } = AlertLevel.Info;

    public bool SuccessfulFastCheckout { get; init; }

    public IImmutableList<CheckInOutCandidate> CheckInOutCandidates { get; init; } = ImmutableList<CheckInOutCandidate>.Empty;

    public IImmutableList<int> AttendanceIds { get; init; } = ImmutableList<int>.Empty;

    public bool FilteredSearchUnsuccessful { get; set; }
}