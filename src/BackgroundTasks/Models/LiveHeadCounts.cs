// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class LiveHeadCounts
    {
        public int LocationId { get; init; }
        public string Location { get; init; } = string.Empty;
        public int KidsCount { get; init; }
        public int VolunteersCount { get; init; }
    }
}