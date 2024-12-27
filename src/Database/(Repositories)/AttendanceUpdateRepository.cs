using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Common;
using KidsTown.Database.EfCore;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Location = KidsTown.Database.EfCore.Location;
using LocationGroup = KidsTown.Shared.LocationGroup;

namespace KidsTown.Database;

public class AttendanceUpdateRepository(IServiceScopeFactory serviceScopeFactory) : IAttendanceUpdateRepository
{
    public async Task<IImmutableList<long>> GetPersistedCheckInsIds(IImmutableList<long> checkinsIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);
        var existingCheckInsIds = await db.Attendances.Where(i => checkinsIds.Contains(i.CheckInsId))
                .Select(i => i.CheckInsId)
                .ToListAsync();

        return existingCheckInsIds.ToImmutableList();
    }

    public async Task<int> InsertAttendances(IImmutableList<CheckInsUpdate> checkInsUpdates)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var guests = checkInsUpdates.Where(c => c.PeopleId == null).ToImmutableList();
        var guestInsertCount = await InsertGuests(guests, db);

        var regularPreCheckIns = checkInsUpdates.Except(guests).ToImmutableList();
        var regularInsertCount = await InsertRegularsAndVolunteers(regularPreCheckIns, db);

        return guestInsertCount + regularInsertCount;
    }

    public async Task<int> AutoCheckInVolunteers()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var volunteers = await db.Attendances
                .Where(
                    a =>
                        a.AttendanceTypeId == (int) AttendanceTypeId.Volunteer
                        && a.CheckInDate == null)
                .ToListAsync();

        volunteers.ForEach(v => v.CheckInDate = DateTime.UtcNow);
        return await db.SaveChangesAsync();
    }

    public async Task<IImmutableList<PersistedLocation>> GetPersistedLocations()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var locations = await db.Locations.Where(l => l.CheckInsLocationId.HasValue)
                .ToListAsync();

        return locations.Select(
                l
                    => new PersistedLocation(l.Id, l.CheckInsLocationId!.Value))
            .ToImmutableList();
    }

    public async Task<int> UpdateLocations(IImmutableList<LocationUpdate> locationUpdates)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var locations = locationUpdates.Select(MapLocation);
        await db.AddRangeAsync(locations);
        return await db.SaveChangesAsync();
    }

    public async Task EnableUnknownLocationGroup()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var locationGroups = await db.LocationGroups.Where(l => l.Id == (int) LocationGroup.Unknown)
                .ToListAsync();

        locationGroups.ForEach(l => l.IsEnabled = true);
        await db.SaveChangesAsync();
    }

    public async Task<int> AutoCheckoutEveryoneByEndOfDay()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendances = await db.Attendances
                .Where(
                    a =>
                        a.CheckInDate != null
                        && a.CheckOutDate == null
                        && a.CheckInDate < DateTime.Today)
                .ToListAsync();

        attendances.ForEach(
            v
                => v.CheckOutDate = v.CheckInDate!.Value.Date.AddDays(value: 1).AddSeconds(value: -1));

        return await db.SaveChangesAsync();
    }

    private static Location MapLocation(LocationUpdate locationUpdate)
    {
        return new Location
        {
            Name = locationUpdate.Name,
            LocationGroupId = 5,
            CheckInsLocationId = locationUpdate.CheckInsLocationId,
            EventId = locationUpdate.EventId
        };
    }

    private static async Task<int> InsertRegularsAndVolunteers(
        IImmutableList<CheckInsUpdate> regularPreCheckIns,
        KidsTownContext db
    )
    {
        var kids = regularPreCheckIns
            .Select(c => c.Kid)
            .GroupBy(p => p.PeopleId)
            .Select(p => p.First())
            .ToImmutableList();

        var existingPeople = await CommonRepository.GetKidsByPeopleIds(
                db,
                kids.Where(p => p.PeopleId.HasValue)
                    .Select(p => p.PeopleId!.Value)
                    .ToImmutableList());

        var peopleUpdates = kids.Where(p => existingPeople.Select(e => e.PeopleId).Contains(p.PeopleId))
            .ToImmutableList();

        var kidsInserts = kids.Except(peopleUpdates).ToImmutableList();
        var insertedPeople = await InsertPeople(db, kidsInserts);

        var checkIns = regularPreCheckIns
            .Select(
                c => MapToAttendance(
                    c,
                    existingPeople.Union(insertedPeople).ToImmutableList()))
            .ToImmutableList();

        await db.AddRangeAsync(checkIns);
        return await db.SaveChangesAsync();
    }

    private async Task<int> InsertGuests(IImmutableList<CheckInsUpdate> guests, DbContext db)
    {
        var existingCheckInsIds =
                await GetPersistedCheckInsIds(guests.Select(g => g.CheckInsId).ToImmutableList());

        var newGuests = guests.Where(g => !existingCheckInsIds.Contains(g.CheckInsId))
            .ToImmutableList();

        var guestAttendances = newGuests.Select(MapGuestAttendance).ToImmutableList();

        await db.AddRangeAsync(guestAttendances);
        return await db.SaveChangesAsync();
    }

    private static async Task<List<Person>> InsertPeople(
        KidsTownContext db,
        IImmutableList<PeopleUpdate> peopleToInsert
    )
    {
        var people = peopleToInsert.Select(MapPerson).ToImmutableList();
        await db.AddRangeAsync(people);
        await db.SaveChangesAsync();

        var insertedPeople = await db.People
                .Where(p => peopleToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync();

        return insertedPeople;
    }

    private static Attendance MapGuestAttendance(CheckInsUpdate guest)
    {
        var person = MapPerson(guest.Kid);
        var parent = MapParent(guest);

        var family = new Family
        {
            Name = "Guest",
            UpdateDate = DateTime.UtcNow,
            People = new List<Person>
            {
                parent
            }
        };

        person.Family = family;

        return MapToAttendance(guest, ImmutableList.Create(person));
    }

    private static Person MapParent(CheckInsUpdate guest)
    {
        var names = guest.EmergencyContactName?.Split(" ");

        var firstName = string.Empty;
        var lastName = string.Empty;

        if (names?.Length == 2)
        {
            firstName = names[0];
            lastName = names[1];
        }
        else if (guest.EmergencyContactName?.Length > 0)
        {
            firstName = guest.EmergencyContactName;
        }

        var adult = new Adult
        {
            PhoneNumber = guest.EmergencyContactNumber,
            IsPrimaryContact = false
        };

        return new Person
        {
            FirstName = firstName,
            LastName = lastName,
            UpdateDate = DateTime.UtcNow,
            Adult = adult
        };
    }

    private static Person MapPerson(PeopleUpdate peopleUpdate)
    {
        return new Person
        {
            PeopleId = peopleUpdate.PeopleId,
            FirstName = peopleUpdate.FirstName,
            LastName = peopleUpdate.LastName,
            UpdateDate = DateTime.UtcNow
        };
    }

    private static Attendance MapToAttendance(CheckInsUpdate checkInsUpdate, IImmutableList<Person> people)
    {
        var person = people.Single(p => p.PeopleId == checkInsUpdate.PeopleId);

        return new Attendance
        {
            CheckInsId = checkInsUpdate.CheckInsId,
            LocationId = checkInsUpdate.LocationId,
            SecurityCode = checkInsUpdate.SecurityCode,
            InsertDate = checkInsUpdate.CreationDate,
            Person = person,
            AttendanceTypeId = MapAttendeeType(checkInsUpdate.AttendeeType)
        };
    }

    private static int MapAttendeeType(AttendeeType attendeeType)
    {
        return attendeeType switch
        {
            AttendeeType.Regular => 1,
            AttendeeType.Guest => 2,
            AttendeeType.Volunteer => 3,
            _ => throw new ArgumentOutOfRangeException(
                nameof(attendeeType),
                attendeeType,
                message: null)
        };
    }
}