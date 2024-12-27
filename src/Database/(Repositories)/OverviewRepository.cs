using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Database.EfCore;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Location = KidsTown.Database.EfCore.Location;

namespace KidsTown.Database;

public class OverviewRepository(IServiceScopeFactory serviceScopeFactory) : IOverviewRepository
{
    public async Task<IImmutableList<Attendee>> GetActiveAttendees(
        IImmutableList<int> selectedLocationGroups,
        long eventId,
        DateTime date)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var people = await (from a in db.Attendances
                    join p in db.People
                        on a.PersonId equals p.Id
                    join at in db.AttendanceTypes
                        on a.AttendanceTypeId equals at.Id
                    join l in db.Locations
                        on a.LocationId equals l.Id
                    where a.InsertDate.Date == date.Date
                        && selectedLocationGroups.Contains(l.LocationGroupId)
                        && l.EventId == eventId
                    select MapAttendee(a, p, at, l))
                .ToListAsync();

        return people.OrderBy(a => a.LastName).ToImmutableList();
    }

    public async Task<IImmutableList<Attendee>> GetAttendanceHistoryByLocations(
        long eventId,
        DateTime startDate,
        DateTime endDate,
        IImmutableList<int> selectedLocations
    )
    {
        return await GetAttendanceHistory(
                eventId,
                startDate,
                endDate,
                selectedLocations);
    }

    public async Task<IImmutableList<Attendee>> GetAttendanceHistoryByLocationGroups(
        long eventId,
        DateTime startDate,
        DateTime endDate,
        IImmutableList<int> selectedLocationGroups
    )
    {
        return await GetAttendanceHistory(
                eventId,
                startDate,
                endDate,
                selectedLocationGroups: selectedLocationGroups);
    }

    private async Task<IImmutableList<Attendee>> GetAttendanceHistory(
        long eventId,
        DateTime startDate,
        DateTime endDate,
        IImmutableList<int>? selectedLocations = null,
        IImmutableList<int>? selectedLocationGroups = null)
    {
        await using var db = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>();
        var attendees = await (from a in db.Attendances
                    join p in db.People
                        on a.PersonId equals p.Id
                    join at in db.AttendanceTypes
                        on a.AttendanceTypeId equals at.Id
                    join l in db.Locations
                        on a.LocationId equals l.Id
                    where ((selectedLocations != null && selectedLocations.Contains(l.Id))
                            || (selectedLocationGroups != null && selectedLocationGroups.Contains(l.LocationGroupId)))
                        && l.EventId == eventId
                        && a.InsertDate.Date >= startDate.Date
                        && a.InsertDate.Date <= endDate.Date
                    select MapAttendee(a, p, at, l))
                .ToListAsync();

        return attendees.ToImmutableList();
    }

    private static Attendee MapAttendee(
        Attendance attendance,
        Person person,
        AttendanceType attendanceType,
        Location location
    )
    {
        var checkState = MappingService.GetCheckState(attendance);

        return new Attendee
        {
            AttendanceId = attendance.Id,
            FamilyId = person.FamilyId,
            FirstName = person.FirstName,
            LastName = person.LastName,
            AttendanceTypeId = (AttendanceTypeId) attendanceType.Id,
            SecurityCode = attendance.SecurityCode,
            LocationGroupId = location.LocationGroupId,
            Location = location.Name,
            CheckState = checkState,
            InsertDate = attendance.InsertDate,
            CheckInDate = attendance.CheckInDate,
            CheckOutDate = attendance.CheckOutDate
        };
    }
}