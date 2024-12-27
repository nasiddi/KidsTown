using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Database.EfCore;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Kid = KidsTown.KidsTown.Models.Kid;

namespace KidsTown.Database;

public class CheckInOutRepository(IServiceScopeFactory serviceScopeFactory) : ICheckInOutRepository
{
    public async Task<IImmutableList<Kid>> GetPeople(
        PeopleSearchParameters peopleSearchParameters)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendances = await db.Attendances
            .Include(a => a.Person)
            .ThenInclude(p => p.Kid)
            .Include(a => a.Location)
            .Where(
                a => a.SecurityCode == peopleSearchParameters.SecurityCode
                    && (peopleSearchParameters.LocationGroups.Contains(a.Location.LocationGroupId)
                        || !peopleSearchParameters.UseFilterLocationGroups)
                    && a.InsertDate >= DateTime.Today
                    && a.Location.EventId == peopleSearchParameters.EventId)
            .ToListAsync();

        return attendances.Select(MapKid).ToImmutableList();
    }

    public async Task<bool> CheckInPeople(IImmutableList<int> attendanceIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendances = await GetAttendances(attendanceIds, db);
        attendances.ForEach(a => a.CheckInDate = DateTime.UtcNow);
        var result = await db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> CheckOutPeople(IImmutableList<int> attendanceIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendances = await GetAttendances(attendanceIds, db);
        attendances.ForEach(a => a.CheckOutDate = DateTime.UtcNow);
        var result = await db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> SetCheckState(CheckState revertedCheckState, IImmutableList<int> attendanceIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendances = await GetAttendances(attendanceIds, db);

        switch (revertedCheckState)
        {
            case CheckState.None:
                var people = await db.People.Where(p => attendances.Select(a => a.PersonId).Contains(p.Id)).ToListAsync();
                var kids = await db.Kids.Where(k => people.Select(p => p.Id).Contains(k.PersonId)).ToListAsync();
                db.RemoveRange(attendances);
                db.RemoveRange(kids);
                db.RemoveRange(people);
                break;
            case CheckState.PreCheckedIn:
                attendances.ForEach(c => c.CheckInDate = null);
                break;
            case CheckState.CheckedIn:
                attendances.ForEach(c => c.CheckOutDate = null);
                break;
            case CheckState.CheckedOut:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(revertedCheckState), revertedCheckState, message: null);
        }

        var result = await db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<int> CreateGuest(int locationId, string securityCode, string firstName, string lastName)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var kid = new EfCore.Kid
        {
            MayLeaveAlone = true,
            HasPeopleWithoutPickupPermission = false
        };

        var person = new Person
        {
            FirstName = firstName,
            LastName = lastName,
            UpdateDate = DateTime.UtcNow,
            Kid = kid
        };

        var attendance = new Attendance
        {
            CheckInsId = 0,
            LocationId = locationId,
            SecurityCode = securityCode,
            AttendanceTypeId = (int) AttendanceTypeId.Guest,
            InsertDate = DateTime.UtcNow,
            Person = person
        };

        var entry = await db.AddAsync(attendance);
        await db.SaveChangesAsync();
        return entry.Entity.Id;
    }

    public async Task<bool> SecurityCodeExists(string securityCode)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendanceCount = await db.Attendances
            .Where(
                a => a.SecurityCode == securityCode
                    && a.InsertDate > DateTime.Today)
            .CountAsync();

        return attendanceCount > 0;
    }

    public async Task<bool> UpdateLocationAndCheckIn(int attendanceId, int locationId)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendance = (await GetAttendances(ImmutableList.Create(attendanceId), db)
            )
            .Single();

        attendance.LocationId = locationId;
        attendance.CheckInDate = DateTime.UtcNow;

        var result = await db.SaveChangesAsync();
        return result > 0;
    }

    private static async Task<List<Attendance>> GetAttendances(
        IImmutableList<int> attendanceIds,
        KidsTownContext db)
    {
        var attendances = await db.Attendances.Where(
                    a =>
                        attendanceIds.Contains(a.Id))
                .ToListAsync();

        return attendances;
    }

    private static Kid MapKid(
        Attendance attendance
    )
    {
        var checkState = MappingService.GetCheckState(attendance);

        return new Kid
        {
            AttendanceId = attendance.Id,
            LocationGroupId = attendance.Location.LocationGroupId,
            FirstName = attendance.Person.FirstName,
            LastName = attendance.Person.LastName,
            MayLeaveAlone = attendance.Person.Kid?.MayLeaveAlone ?? true,
            HasPeopleWithoutPickupPermission = attendance.Person.Kid?.HasPeopleWithoutPickupPermission ?? false,
            CheckState = checkState
        };
    }
}