using System;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class HeadCounts
    {
        public DateTime Date { get; set; }
        public int LocationId { get; set; }
        public int RegularCount { get; set; }
        public int GuestCount { get; set; }
        public int VolunteerCount { get; set; }
        public int PreCheckInOnlyCount { get; set; }
        public int NoCheckOutCount { get; set; }
    }
}