using System.Collections.Immutable;
using KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown;

public class OverviewService(IOverviewRepository overviewRepository, IPeopleRepository peopleRepository)
    : IOverviewService
{
    public async Task<IImmutableList<AttendeesByLocation>> GetActiveAttendees(
        long eventId,
        IImmutableList<int> selectedLocationGroups,
        DateTime date)
    {
        var attendees = await overviewRepository.GetActiveAttendees(
            selectedLocationGroups,
            eventId,
            date);

        var familyIds = attendees.Where(a => a.FamilyId.HasValue)
            .Select(a => a.FamilyId!.Value)
            .ToImmutableList();

        var adults = await peopleRepository.GetAdults(familyIds);

        var attendeesWithAdultsInfo = attendees.Select(
            attendee =>
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

        return attendeesWithAdultsInfo.GroupBy(a => a.Location)
            .Select(MapAttendeesByLocation)
            .ToImmutableList();
    }

    public async Task<IImmutableList<HeadCounts>> GetSummedUpHeadCounts(
        long eventId,
        IImmutableList<int> selectedLocations,
        DateTime startDate,
        DateTime endDate)
    {
        var attendees = await overviewRepository.GetAttendanceHistoryByLocations(
                selectedLocations: selectedLocations,
                eventId: eventId,
                startDate: startDate,
                endDate: endDate);

        return attendees.GroupBy(a => a.InsertDate.Date)
            .Select(MapDailyStatistic)
            .ToImmutableList();
    }

    public async Task<IImmutableList<LiveHeadCounts>> GetHeadCountsByLocations(
        long eventId,
        IImmutableList<int> selectedLocationGroups,
        DateTime startDate,
        DateTime endDate
    )
    {
        var attendees = await overviewRepository.GetAttendanceHistoryByLocationGroups(
                selectedLocationGroups: selectedLocationGroups,
                eventId: eventId,
                startDate: startDate,
                endDate: endDate);

        return attendees.GroupBy(a => a.Location)
            .OrderBy(g => g.First().LocationGroupId)
            .ThenBy(g => g.First().Location)
            .Select(l => MapLiveStatistics(l.ToImmutableList()))
            .ToImmutableList();
    }

    private static LiveHeadCounts MapLiveStatistics(IImmutableList<Attendee> attendees)
    {
        var regularCounts = GetCounts(attendees, AttendanceTypeId.Regular);
        var guestCounts = GetCounts(attendees, AttendanceTypeId.Guest);
        var volunteerCounts = GetCounts(attendees, AttendanceTypeId.Volunteer);

        return new LiveHeadCounts
        {
            Location = attendees[index: 0].Location,
            KidsCount = regularCounts.CheckedIn + guestCounts.CheckedIn,
            VolunteersCount = volunteerCounts.CheckedIn
        };
    }

    private static AttendeesByLocation MapAttendeesByLocation(IGrouping<string, Attendee> attendees)
    {
        var volunteers = attendees.Where(a => a.AttendanceTypeId == AttendanceTypeId.Volunteer).ToImmutableList();
        var kids = attendees.Where(a => a.AttendanceTypeId != AttendanceTypeId.Volunteer).ToImmutableList();

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
        var regularCounts = GetCounts(attendeesByDate, AttendanceTypeId.Regular);
        var guestCounts = GetCounts(attendeesByDate, AttendanceTypeId.Guest);
        var volunteerCounts = GetCounts(attendeesByDate, AttendanceTypeId.Volunteer);

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

    private static Counts GetCounts(IImmutableList<Attendee> attendees, AttendanceTypeId attendanceTypeId)
    {
        var preCheckedIn = attendees.Where(a => a.AttendanceTypeId == attendanceTypeId).ToImmutableList();
        var checkedIn = preCheckedIn.Where(a => a.CheckState > CheckState.PreCheckedIn).ToImmutableList();
        var checkedOut = checkedIn.Where(a => a.CheckState == CheckState.CheckedOut).ToImmutableList();
        var autoCheckedOutCount = checkedOut.Count(
            a
                => a.CheckOutDate!.Value.TimeOfDay >= TimeSpan.FromDays(value: 1).Subtract(TimeSpan.FromSeconds(value: 1)));

        return new Counts(
            preCheckedIn.Count,
            checkedIn.Count,
            checkedOut.Count,
            autoCheckedOutCount);
    }

    private record Counts(int preCheckedIn, int checkedIn, int checkedOut, int autoCheckedOut)
    {
        public readonly int Attendance = checkedIn;

        public int PreCheckedIn => preCheckedIn - Attendance;

        public int CheckedIn => Attendance - checkedOut;

        public int NoOrAutoCheckOut => Attendance - (checkedOut - autoCheckedOut);
    }
}