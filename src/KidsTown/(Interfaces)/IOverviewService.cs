using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IOverviewService
    {
        Task<ImmutableList<AttendeesByLocation>> GetActiveAttendees(long eventId, IImmutableList<int> selectedLocationGroups,
            DateTime date);

        Task<ImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate);

        Task<ImmutableList<LiveHeadCounts>> GetHeadCountsByLocations(
            long eventId,
            IImmutableList<int> selectedLocationGroups,
            DateTime startDate,
            DateTime endDate
        );
    }
}