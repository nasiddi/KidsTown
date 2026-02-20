using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IOverviewService
{
    Task<IImmutableList<AttendeesByLocation>> GetActiveAttendees(
        long eventId,
        IImmutableList<int> selectedLocationGroups,
        DateTime date);

    Task<IImmutableList<HeadCounts>> GetSummedUpHeadCounts(
        long eventId,
        IImmutableList<int> selectedLocations,
        DateTime startDate,
        DateTime endDate);

    Task<IImmutableList<LiveHeadCounts>> GetHeadCountsByLocations(
        long eventId,
        IImmutableList<int> selectedLocationGroups,
        DateTime startDate,
        DateTime endDate
    );
}