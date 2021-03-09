using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using Attendee = CheckInsExtension.CheckInUpdateJobs.Models.Attendee;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public class OverviewService : IOverviewService
    {
        private readonly IOverviewRepository _overviewRepository;

        public OverviewService(IOverviewRepository overviewRepository)
        {
            _overviewRepository = overviewRepository;
        }

        public async Task<ImmutableList<AttendeesByLocation>> GetActiveAttendees(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime date)
        {
            var attendees = await _overviewRepository.GetActiveAttendees(selectedLocations, eventId, date);
            return attendees.GroupBy(a => a.LocationId).Select(MapAttendeesByLocation).ToImmutableList();
        }

        private static AttendeesByLocation MapAttendeesByLocation(IGrouping<int, Attendee> attendees)
        {
            var volunteers = attendees.Where(a => a.AttendanceType == AttendanceTypes.Volunteer).ToImmutableList();
            var kids = attendees.Where(a => a.AttendanceType != AttendanceTypes.Volunteer).ToImmutableList();

            return new AttendeesByLocation
            {
                LocationId = attendees.Key,
                Kids = kids,
                Volunteers = volunteers
            };
        }

        public async Task<ImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate)
        {
            var headCounts = await GetHeadCounts(eventId, selectedLocations, startDate, DateTime.Today);
            return headCounts.GroupBy(h => h.Date).Select(SumUpDay).ToImmutableList();
        }

        private static HeadCounts SumUpDay(IGrouping<DateTime, HeadCounts> headCounts) =>
            new()
            {
                Date = headCounts.Key,
                LocationId = 0,
                RegularCount = headCounts.Sum(h => h.RegularCount),
                GuestCount = headCounts.Sum(h => h.GuestCount),
                VolunteerCount = headCounts.Sum(h => h.VolunteerCount),
                PreCheckInOnlyCount = headCounts.Sum(h => h.PreCheckInOnlyCount),
                NoCheckOutCount = headCounts.Sum(h => h.NoCheckOutCount)
            };

        public async Task<ImmutableList<HeadCounts>> GetHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate,
            DateTime endDate)
        {
            var attendees = await _overviewRepository.GetAttendanceHistory(selectedLocations, eventId, startDate, endDate);
            return attendees.GroupBy(a => a.LocationId).SelectMany(MapHeadCounts).ToImmutableList();
        }

        private static ImmutableList<HeadCounts> MapHeadCounts(IGrouping<int, Attendee> attendeesByLocation)
        {
            return attendeesByLocation.GroupBy(a => a.InsertDate.Date).Select(MapDailyStatistic).ToImmutableList();
        }

        private static HeadCounts MapDailyStatistic(IGrouping<DateTime, Attendee> attendees)
        {
            var regularCounts = GetCounts(attendees, AttendanceTypes.Regular);
            var guestCounts = GetCounts(attendees, AttendanceTypes.Guest);
            var volunteerCounts = GetCounts(attendees, AttendanceTypes.Volunteer);

            return new HeadCounts
            {
                Date = attendees.Key.AddHours(12),
                LocationId = attendees.First().LocationId,
                RegularCount = regularCounts.CheckedIn,
                GuestCount = guestCounts.CheckedIn,
                VolunteerCount = volunteerCounts.CheckedIn,
                PreCheckInOnlyCount = regularCounts.PreCheckedInOnly + guestCounts.PreCheckedInOnly,
                NoCheckOutCount = regularCounts.NoCheckOut + guestCounts.NoCheckOut
            };
        }

        private static Counts GetCounts(IGrouping<DateTime, Attendee> attendees, AttendanceTypes attendanceType)
        {
            var preCheckedIn = attendees.Where(a => a.AttendanceType == attendanceType).ToImmutableList();
            var checkedIn = preCheckedIn.Where(a => a.CheckState > CheckState.PreCheckedIn ).ToImmutableList();
            var checkedOutCount = checkedIn.Count(a => a.CheckState == CheckState.CheckedOut
                                                       && a.CheckOutDate!.Value.TimeOfDay < TimeSpan.FromDays(1).Subtract(TimeSpan.FromSeconds(1)));

            return new Counts(preCheckedIn.Count, checkedIn.Count, checkedOutCount);
        }

        private class Counts
        {
            public readonly int PreCheckedIn;
            public readonly int CheckedIn;
            public readonly int CheckedOut;

            public Counts(int preCheckedIn, int checkedIn, int checkedOut)
            {
                PreCheckedIn = preCheckedIn;
                CheckedIn = checkedIn;
                CheckedOut = checkedOut;
            }

            public int PreCheckedInOnly => PreCheckedIn - CheckedIn;
            public int NoCheckOut => CheckedIn - CheckedOut;
        }
    }
}