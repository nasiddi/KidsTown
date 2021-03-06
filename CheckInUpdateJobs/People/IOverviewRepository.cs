using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IOverviewRepository
    {
        Task<ImmutableList<Attendee>> GetActiveAttendees(
            IImmutableList<int> selectedLocations,
            IImmutableList<long> eventIds);

        Task<ImmutableList<Attendee>> GetAttendanceHistory(
            IImmutableList<int> selectedLocations, 
            IImmutableList<long> eventIds);
    }
}