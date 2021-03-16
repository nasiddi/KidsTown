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
            IImmutableList<int> selectedLocationGroups,
            DateTime date)
        {
            var attendees = await _overviewRepository.GetActiveAttendees(selectedLocationGroups: selectedLocationGroups, eventId: eventId, date: date)
                .ConfigureAwait(continueOnCapturedContext: false);
            return attendees.GroupBy(keySelector: a => a.Location).Select(selector: MapAttendeesByLocation).ToImmutableList();
        }

        private static AttendeesByLocation MapAttendeesByLocation(IGrouping<string, Attendee> attendees)
        {
            var volunteers = attendees.Where(predicate: a => a.AttendanceType == AttendanceTypes.Volunteer).ToImmutableList();
            var kids = attendees.Where(predicate: a => a.AttendanceType != AttendanceTypes.Volunteer).ToImmutableList();

            return new AttendeesByLocation
            {
                Location = attendees.Key,
                LocationGroupId = kids.FirstOrDefault()?.LocationGroupId ?? volunteers.First().LocationGroupId,
                Kids = kids,
                Volunteers = volunteers
            };
        }

        public async Task<ImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocationGroups,
            DateTime startDate)
        {
            var headCounts = await GetHeadCounts(eventId: eventId, selectedLocations: selectedLocationGroups, startDate: startDate, endDate: DateTime.Today)
                .ConfigureAwait(continueOnCapturedContext: false);
            return headCounts.GroupBy(keySelector: h => h.Date).Select(selector: SumUpDay).ToImmutableList();
        }

        private static HeadCounts SumUpDay(IGrouping<DateTime, HeadCounts> headCounts) =>
            new()
            {
                Date = headCounts.Key,
                LocationId = 0,
                RegularCount = headCounts.Sum(selector: h => h.RegularCount),
                GuestCount = headCounts.Sum(selector: h => h.GuestCount),
                VolunteerCount = headCounts.Sum(selector: h => h.VolunteerCount),
                PreCheckInOnlyCount = headCounts.Sum(selector: h => h.PreCheckInOnlyCount),
                NoCheckOutCount = headCounts.Sum(selector: h => h.NoCheckOutCount)
            };

        public async Task<ImmutableList<HeadCounts>> GetHeadCountsByLocations(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate,
            DateTime endDate)
        {
            var attendees = await _overviewRepository.GetAttendanceHistory(selectedLocationGroups: selectedLocations, eventId: eventId, startDate: startDate, endDate: endDate)
                .ConfigureAwait(continueOnCapturedContext: false);
            return attendees.GroupBy(keySelector: a => a.Location).SelectMany(selector: MapHeadCounts)
                .OrderBy(keySelector: c => c.LocationId)
                .ThenBy(keySelector: c => c.Location).ToImmutableList();
        }
        
        private async Task<ImmutableList<HeadCounts>> GetHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate,
            DateTime endDate)
        {
            var attendees = await _overviewRepository.GetAttendanceHistory(selectedLocationGroups: selectedLocations, eventId: eventId, startDate: startDate, endDate: endDate)
                .ConfigureAwait(continueOnCapturedContext: false);
            return attendees.GroupBy(keySelector: a => a.LocationGroupId).SelectMany(selector: MapHeadCounts).ToImmutableList();
        }

        private static ImmutableList<HeadCounts> MapHeadCounts<T>(IGrouping<T, Attendee> attendeesByLocation)
        {
            return attendeesByLocation.GroupBy(keySelector: a => a.InsertDate.Date).Select(selector: MapDailyStatistic).ToImmutableList();
        }

        private static HeadCounts MapDailyStatistic(IGrouping<DateTime, Attendee> attendees)
        {
            var attendeesByDate = attendees.ToImmutableList();
            var regularCounts = GetCounts(attendees: attendeesByDate, attendanceType: AttendanceTypes.Regular);
            var guestCounts = GetCounts(attendees: attendeesByDate, attendanceType: AttendanceTypes.Guest);
            var volunteerCounts = GetCounts(attendees: attendeesByDate, attendanceType: AttendanceTypes.Volunteer);

            return new HeadCounts
            {
                Date = attendees.Key.AddHours(value: 12),
                LocationId = attendees.First().LocationGroupId,
                Location = attendees.First().Location,
                RegularCount = regularCounts.CheckedIn,
                GuestCount = guestCounts.CheckedIn,
                VolunteerCount = volunteerCounts.CheckedIn,
                PreCheckInOnlyCount = regularCounts.PreCheckedInOnly + guestCounts.PreCheckedInOnly,
                NoCheckOutCount = regularCounts.NoCheckOut + guestCounts.NoCheckOut
            };
        }

        private static Counts GetCounts(IImmutableList<Attendee> attendees, AttendanceTypes attendanceType)
        {
            var preCheckedIn = attendees.Where(predicate: a => a.AttendanceType == attendanceType).ToImmutableList();
            var checkedIn = preCheckedIn.Where(predicate: a => a.CheckState > CheckState.PreCheckedIn ).ToImmutableList();
            var checkedOutCount = checkedIn.Count(predicate: a => a.CheckState == CheckState.CheckedOut
                                                                  && a.CheckOutDate!.Value.TimeOfDay < TimeSpan.FromDays(value: 1).Subtract(ts: TimeSpan.FromSeconds(value: 1)));

            return new Counts(preCheckedIn: preCheckedIn.Count, checkedIn: checkedIn.Count, checkedOut: checkedOutCount);
        }

        private class Counts
        {
            private readonly int _preCheckedIn;
            public readonly int CheckedIn;
            private readonly int _checkedOut;

            public Counts(int preCheckedIn, int checkedIn, int checkedOut)
            {
                _preCheckedIn = preCheckedIn;
                CheckedIn = checkedIn;
                _checkedOut = checkedOut;
            }

            public int PreCheckedInOnly => _preCheckedIn - CheckedIn;
            public int NoCheckOut => CheckedIn - _checkedOut;
        }
    }
}