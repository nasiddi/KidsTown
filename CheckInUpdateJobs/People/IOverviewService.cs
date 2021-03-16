using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IOverviewService
    {
        Task<ImmutableList<AttendeesByLocation>> GetActiveAttendees(long eventId, IImmutableList<int> selectedLocationGroups,
            DateTime date);

        Task<ImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocationGroups,
            DateTime startDate);

        Task<ImmutableList<HeadCounts>> GetHeadCountsByLocations(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate,
            DateTime endDate);
    }
}