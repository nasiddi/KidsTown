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
        private readonly IConfigurationService _configurationService;

        public OverviewService(IOverviewRepository overviewRepository, IConfigurationService configurationService)
        {
            _overviewRepository = overviewRepository;
            _configurationService = configurationService;
        }

        public async Task<ImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocations)
        {
            return await _overviewRepository.GetActiveAttendees(selectedLocations, _configurationService.GetEventIds());
        }

        public async Task<ImmutableList<DailyStatistic>> GetAttendanceHistory(IImmutableList<int> selectedLocations)
        {
            var attendees = await _overviewRepository.GetAttendanceHistory(selectedLocations, _configurationService.GetEventIds());

            return attendees.GroupBy(a => a.InsertDate.Date).Select(MapDailyStatistic).ToImmutableList();
        }

        private DailyStatistic MapDailyStatistic(IGrouping<DateTime, Attendee> attendees)
        {
            var regularCounts = GetCounts(attendees, AttendanceTypes.Regular);
            var guestCounts = GetCounts(attendees, AttendanceTypes.Guest);
            var volunteerCounts = GetCounts(attendees, AttendanceTypes.Volunteer);

            return new DailyStatistic
            {
                Date = attendees.Key,
                RegularCount = regularCounts.CheckedIn,
                GuestCount = guestCounts.CheckedIn,
                VolunteerCount = volunteerCounts.CheckedIn,
                PreCheckInOnlyCount = regularCounts.PreCheckedInOnly + guestCounts.PreCheckedInOnly,
                NoCheckOutCount = regularCounts.NoCheckOut + guestCounts.NoCheckOut
            };
        }

        private Counts GetCounts(IGrouping<DateTime, Attendee> attendees, AttendanceTypes attendanceType)
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