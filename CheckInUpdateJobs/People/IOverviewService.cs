using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IOverviewService
    {
        Task<ImmutableList<AttendeesByLocation>> GetActiveAttendees(long eventId, IImmutableList<int> selectedLocations,
            DateTime date);

        Task<ImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate);
        Task<ImmutableList<HeadCounts>> GetHeadCounts(
            long eventId, 
            IImmutableList<int> selectedLocations, 
            DateTime startDate, 
            DateTime endDate);
    }
}