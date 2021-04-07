using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.KidsTown.Models
{
    public class HeadCounts
    {
        public DateTime Date { get; init; }
        public int RegularCount { get; init; }
        public int GuestCount { get; init; }
        public int VolunteerCount { get; init; }
        public int PreCheckInOnlyCount { get; init; }
        public int NoCheckOutCount { get; init; }
    }
}