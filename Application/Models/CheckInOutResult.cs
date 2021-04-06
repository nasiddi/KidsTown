using System.Collections.Immutable;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Application.Models
{
    public class CheckInOutResult
    {
        public string Text { get; init; } = string.Empty;
        public AlertLevel AlertLevel { get; init; } = AlertLevel.Info;
        public bool SuccessfulFastCheckout { get; init; }
        public ImmutableList<CheckInOutCandidate> CheckInOutCandidates { get; init; } = ImmutableList<CheckInOutCandidate>.Empty;
        public ImmutableList<int> AttendanceIds { get; init; } = ImmutableList<int>.Empty;
    }
}