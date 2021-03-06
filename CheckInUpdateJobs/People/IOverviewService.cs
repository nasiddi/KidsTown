using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IOverviewService
    {
        Task<ImmutableList<Attendee>> GetActiveAttendees(long eventId, IImmutableList<int> selectedLocations);
        Task<ImmutableList<DailyStatistic>> GetAttendanceHistory(long eventId, IImmutableList<int> selectedLocations);
    }
}