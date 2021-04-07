using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IOverviewRepository
    {
        Task<ImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocationGroups,
            long eventId, DateTime date);

        Task<ImmutableList<Attendee>> GetAttendanceHistory(
            long eventId,
            DateTime startDate,
            DateTime endDate,
            IImmutableList<int> selectedLocations = null!,
            IImmutableList<int> selectedLocationGroups = null!
        );
    }
}