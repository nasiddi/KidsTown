using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IOverviewRepository
{
    Task<IImmutableList<Attendee>> GetActiveAttendees(
        IImmutableList<int> selectedLocationGroups,
        long eventId,
        DateTime date);

    Task<IImmutableList<Attendee>> GetAttendanceHistoryByLocations(
        long eventId,
        DateTime startDate,
        DateTime endDate,
        IImmutableList<int> selectedLocations
    );

    Task<IImmutableList<Attendee>> GetAttendanceHistoryByLocationGroups(
        long eventId,
        DateTime startDate,
        DateTime endDate,
        IImmutableList<int> selectedLocationGroups
    );
}