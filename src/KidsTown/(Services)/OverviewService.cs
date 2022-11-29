using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown.KidsTown;

public class OverviewService : IOverviewService
{
    private readonly IOverviewRepository _overviewRepository;
    private readonly IPeopleRepository _peopleRepository;

    public OverviewService(IOverviewRepository overviewRepository, IPeopleRepository peopleRepository)
    {
        _overviewRepository = overviewRepository;
        _peopleRepository = peopleRepository;
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

        var familyIds = attendees.Where(predicate: a => a.FamilyId.HasValue)
            .Select(selector: a => a.FamilyId!.Value).
            ToImmutableList();
            
        var adults = await _peopleRepository.GetAdults(familyIds: familyIds);

        var attendeesWithAdultsInfo = attendees.Select(selector: attendee =>
        {
            var adultInfos = adults.Where(predicate: a => a.FamilyId == attendee.FamilyId).ToImmutableList();
            if (adultInfos.Count == 0)
            {
                return attendee;
            }

            return new()
            {
                AttendanceId = attendee.AttendanceId,
                FamilyId = attendee.FamilyId,
                FirstName = attendee.FirstName,
                LastName = attendee.LastName,
                AttendanceTypeId = attendee.AttendanceTypeId,
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

    public async Task<IImmutableList<HeadCounts>> GetSummedUpHeadCounts(long eventId,
        IImmutableList<int> selectedLocations,
        DateTime startDate,
        DateTime endDate)
    {
        var attendees = await _overviewRepository.GetAttendanceHistoryByLocations(
                selectedLocations: selectedLocations,
                eventId: eventId,
                startDate: startDate,
                endDate: endDate)
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
            .OrderBy(keySelector: g => g.First().LocationGroupId)
            .ThenBy(keySelector: g => g.First().Location)
            .Select(selector: l => MapLiveStatistics(attendees: l.ToImmutableList()))
            .ToImmutableList();
    }

    private static LiveHeadCounts MapLiveStatistics(IImmutableList<Attendee> attendees)
    {
        var regularCounts = GetCounts(attendees: attendees, attendanceTypeId: AttendanceTypeId.Regular);
        var guestCounts = GetCounts(attendees: attendees, attendanceTypeId: AttendanceTypeId.Guest);
        var volunteerCounts = GetCounts(attendees: attendees, attendanceTypeId: AttendanceTypeId.Volunteer);

        return new()
        {
            Location = attendees[index: 0].Location,
            KidsCount = regularCounts.CheckedIn + guestCounts.CheckedIn,
            VolunteersCount = volunteerCounts.CheckedIn
        };
    }
        
    private static AttendeesByLocation MapAttendeesByLocation(IGrouping<string, Attendee> attendees)
    {
        var volunteers = attendees.Where(predicate: a => a.AttendanceTypeId == AttendanceTypeId.Volunteer).ToImmutableList();
        var kids = attendees.Where(predicate: a => a.AttendanceTypeId != AttendanceTypeId.Volunteer).ToImmutableList();

        return new()
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
        var regularCounts = GetCounts(attendees: attendeesByDate, attendanceTypeId: AttendanceTypeId.Regular);
        var guestCounts = GetCounts(attendees: attendeesByDate, attendanceTypeId: AttendanceTypeId.Guest);
        var volunteerCounts = GetCounts(attendees: attendeesByDate, attendanceTypeId: AttendanceTypeId.Volunteer);

        return new()
        {
            Date = attendees.Key.AddHours(value: 12),
            RegularCount = regularCounts.Attendance,
            GuestCount = guestCounts.Attendance,
            VolunteerCount = volunteerCounts.Attendance,
            PreCheckInOnlyCount = regularCounts.PreCheckedIn + guestCounts.PreCheckedIn,
            NoCheckOutCount = regularCounts.NoOrAutoCheckOut + guestCounts.NoOrAutoCheckOut
        };
    }

    private static Counts GetCounts(IImmutableList<Attendee> attendees, AttendanceTypeId attendanceTypeId)
    {
        var preCheckedIn = attendees.Where(predicate: a => a.AttendanceTypeId == attendanceTypeId).ToImmutableList();
        var checkedIn = preCheckedIn.Where(predicate: a => a.CheckState > CheckState.PreCheckedIn ).ToImmutableList();
        var checkedOut = checkedIn.Where(predicate: a => a.CheckState == CheckState.CheckedOut).ToImmutableList();
        var autoCheckedOutCount = checkedOut.Count(predicate: a 
            => a.CheckOutDate!.Value.TimeOfDay >= TimeSpan.FromDays(value: 1).Subtract(ts: TimeSpan.FromSeconds(value: 1)));

        return new(
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