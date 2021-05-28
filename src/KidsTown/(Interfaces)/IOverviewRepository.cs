using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IOverviewRepository
    {
        Task<IImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocationGroups,
            long eventId, DateTime date);

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
}