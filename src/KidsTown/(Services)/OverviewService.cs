using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using Attendee = KidsTown.KidsTown.Models.Attendee;

namespace KidsTown.KidsTown
{
    public class OverviewService : IOverviewService
    {
        private readonly IOverviewRepository _overviewRepository;

        public OverviewService(IOverviewRepository overviewRepository)
        {
            _overviewRepository = overviewRepository;
        }

        public async Task<IImmutableList<AttendeesByLocation>> GetActiveAttendees(
            long eventId,
            IImmutableList<int> selectedLocationGroups,
            DateTime date)
        {
            var attendees = await _overviewRepository.GetActiveAttendees(
                    selectedLocationGroups: selectedLocationGroups,
                    eventId: eventId,
                    date: date)
                .ConfigureAwait(continueOnCapturedContext: false);

            var familyIds = attendees.Where(a => a.FamilyId.HasValue)
                .Select(a => a.FamilyId!.Value).
                ToImmutableList();
            
            var adults = await _overviewRepository.GetAdults(familyIds);

            var attendeesWithAdultsInfo = attendees.Select(attendee =>
            {
                var adultInfos = adults.Where(a => a.FamilyId == attendee.FamilyId).ToImmutableList();
                if (adultInfos.Count == 0)
                {
                    return attendee;
                }

                return new Attendee
                {
                    AttendanceId = attendee.AttendanceId,
                    FamilyId = attendee.FamilyId,
                    FirstName = attendee.FirstName,
                    LastName = attendee.LastName,
                    AttendanceType = attendee.AttendanceType,
                    LocationGroupId = attendee.LocationGroupId,
                    Location = attendee.Location,
                    SecurityCode = attendee.SecurityCode,
                    CheckState = attendee.CheckState,
                    InsertDate = attendee.InsertDate,
                    CheckInDate = attendee.CheckInDate,
                    CheckOutDate = attendee.CheckOutDate,
                    Adults = adultInfos
                };

            });
            
            return attendeesWithAdultsInfo.GroupBy(keySelector: a => a.Location)
                .Select(selector: MapAttendeesByLocation)
                .ToImmutableList();
        }

        public async Task<IImmutableList<HeadCounts>> GetSummedUpHeadCounts(
            long eventId,
            IImmutableList<int> selectedLocations,
            DateTime startDate)
        {
            var attendees = await _overviewRepository.GetAttendanceHistoryByLocations(
                    selectedLocations: selectedLocations,
                    eventId: eventId,
                    startDate: startDate,
                    endDate: DateTime.Today)
                .ConfigureAwait(continueOnCapturedContext: false);
            
            return attendees.GroupBy(keySelector: a => a.InsertDate.Date)
                .Select(selector: MapDailyStatistic)
                .ToImmutableList();
        }

        public async Task<IImmutableList<LiveHeadCounts>> GetHeadCountsByLocations(
            long eventId,
            IImmutableList<int> selectedLocationGroups,
            DateTime startDate,
            DateTime endDate
        )
        {
            var attendees = await _overviewRepository.GetAttendanceHistoryByLocationGroups(
                    selectedLocationGroups: selectedLocationGroups,
                    eventId: eventId,
                    startDate: startDate,
                    endDate: endDate)
                .ConfigureAwait(continueOnCapturedContext: false);
            
            return attendees.GroupBy(keySelector: a => a.Location)
                .Select(selector: l => MapLiveStatistics(attendees: l.ToImmutableList()))
                .OrderBy(keySelector: c => c.LocationId)
                .ThenBy(keySelector: c => c.Location).ToImmutableList();
        }

        private static LiveHeadCounts MapLiveStatistics(IImmutableList<Attendee> attendees)
        {
            var regularCounts = GetCounts(attendees: attendees, attendanceType: AttendanceTypes.Regular);
            var guestCounts = GetCounts(attendees: attendees, attendanceType: AttendanceTypes.Guest);
            var volunteerCounts = GetCounts(attendees: attendees, attendanceType: AttendanceTypes.Volunteer);

            return new LiveHeadCounts
            {
                LocationId = attendees[index: 0].LocationGroupId,
                Location = attendees[index: 0].Location,
                KidsCount = regularCounts.CheckedIn + guestCounts.CheckedIn,
                VolunteersCount = volunteerCounts.CheckedIn
            };
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

        private static HeadCounts MapDailyStatistic(IGrouping<DateTime, Attendee> attendees)
        {
            var attendeesByDate = attendees.ToImmutableList();
            var regularCounts = GetCounts(attendees: attendeesByDate, attendanceType: AttendanceTypes.Regular);
            var guestCounts = GetCounts(attendees: attendeesByDate, attendanceType: AttendanceTypes.Guest);
            var volunteerCounts = GetCounts(attendees: attendeesByDate, attendanceType: AttendanceTypes.Volunteer);

            return new HeadCounts
            {
                Date = attendees.Key.AddHours(value: 12),
                RegularCount = regularCounts.Attendance,
                GuestCount = guestCounts.Attendance,
                VolunteerCount = volunteerCounts.Attendance,
                PreCheckInOnlyCount = regularCounts.PreCheckedIn + guestCounts.PreCheckedIn,
                NoCheckOutCount = regularCounts.NoOrAutoCheckOut + guestCounts.NoOrAutoCheckOut
            };
        }

        private static Counts GetCounts(IImmutableList<Attendee> attendees, AttendanceTypes attendanceType)
        {
            var preCheckedIn = attendees.Where(predicate: a => a.AttendanceType == attendanceType).ToImmutableList();
            var checkedIn = preCheckedIn.Where(predicate: a => a.CheckState > CheckState.PreCheckedIn ).ToImmutableList();
            var checkedOut = checkedIn.Where(predicate: a => a.CheckState == CheckState.CheckedOut).ToImmutableList();
            var autoCheckedOutCount = checkedOut.Count(predicate: a 
                => a.CheckOutDate!.Value.TimeOfDay >= TimeSpan.FromDays(value: 1).Subtract(ts: TimeSpan.FromSeconds(value: 1)));

            return new Counts(
                preCheckedIn: preCheckedIn.Count,
                checkedIn: checkedIn.Count,
                checkedOut: checkedOut.Count,
                autoCheckedOut: autoCheckedOutCount);
        }

        private class Counts
        {
            private readonly int _preCheckedIn;
            public readonly int Attendance;
            private readonly int _checkedOut;
            private readonly int _autoCheckedOut;

            public Counts(int preCheckedIn, int checkedIn, int checkedOut, int autoCheckedOut)
            {
                _preCheckedIn = preCheckedIn;
                Attendance = checkedIn;
                _checkedOut = checkedOut;
                _autoCheckedOut = autoCheckedOut;
            }

            public int PreCheckedIn => _preCheckedIn - Attendance;
            public int CheckedIn => Attendance - _checkedOut;

            public int NoOrAutoCheckOut => Attendance - (_checkedOut - _autoCheckedOut);
        }
    }
}