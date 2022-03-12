using System.Collections.Immutable;
using KidsTown.Shared;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace KidsTown.Application.Models;

public class CheckInOutRequest
{
    public string SecurityCode { get; init; } = string.Empty;
    public long EventId { get; init; }
    public IImmutableList<int> SelectedLocationGroupIds { get; init; } = ImmutableList<int>.Empty;
    public bool IsFastCheckInOut { get; init; } = true;
    public CheckType CheckType { get; init; }
    public IImmutableList<CheckInOutCandidate> CheckInOutCandidates { get; init; } = ImmutableList<CheckInOutCandidate>.Empty;
    public string Guid { get; set; } = string.Empty;
    public bool FilterLocations { get; set; }
}