using System;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class HeadCounts
    {
        public DateTime Date { get; init; }
        public int LocationId { get; init; }
        public string Location { get; init; } = string.Empty;
        public int RegularCount { get; init; }
        public int GuestCount { get; init; }
        public int VolunteerCount { get; init; }
        public int PreCheckInOnlyCount { get; init; }
        public int NoCheckOutCount { get; init; }
    }
}