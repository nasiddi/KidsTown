using System;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class CheckInsEvent
    {
        public long EventId { get; init; }
        public string Name { get; init; } = string.Empty;
        public Uri Link { get; init; }
    }
}