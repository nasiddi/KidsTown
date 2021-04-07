using System.Collections.Immutable;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Application.Models
{
    public class CheckInOutRequest
    {
        public string SecurityCode { get; init; } = string.Empty;
        public long EventId { get; init; }
        public IImmutableList<int> SelectedLocationIds { get; init; } = ImmutableList<int>.Empty;
        public bool IsFastCheckInOut { get; init; } = true;
        public CheckType CheckType { get; init; }
        public IImmutableList<CheckInOutCandidate> CheckInOutCandidates { get; init; } = ImmutableList<CheckInOutCandidate>.Empty;
    }
}