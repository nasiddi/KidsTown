using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IOverviewService
    {
        Task<IImmutableList<AttendeesByLocation>> GetActiveAttendees(
            long eventId, 
            IImmutableList<int> selectedLocationGroups,
            DateTime date);

        Task<IImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate);

        Task<IImmutableList<LiveHeadCounts>> GetHeadCountsByLocations(
            long eventId,
            IImmutableList<int> selectedLocationGroups,
            DateTime startDate,
            DateTime endDate
        );
    }
}