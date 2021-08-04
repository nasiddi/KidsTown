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

namespace KidsTown.Database
{
    public class AttendanceUpdateRepository : IAttendanceUpdateRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AttendanceUpdateRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task<IImmutableList<long>> GetPersistedCheckInsIds(IImmutableList<long> checkinsIds)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            var existingCheckInsIds = await db.Attendances.Where(predicate: i => checkinsIds.Contains(i.CheckInsId))
                .Select(selector: i => i.CheckInsId)
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return existingCheckInsIds.ToImmutableList();
        }
        
        public async Task<int> InsertAttendances(IImmutableList<CheckInsUpdate> checkInsUpdates)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var guests = checkInsUpdates.Where(predicate: c => c.PeopleId == null).ToImmutableList();
            var guestInsertCount = await InsertGuests(guests: guests, db: db).ConfigureAwait(continueOnCapturedContext: false);

            var regularPreCheckIns = checkInsUpdates.Except(second: guests).ToImmutableList();
            var regularInsertCount = await InsertRegularsAndVolunteers(regularPreCheckIns: regularPreCheckIns, db: db)
                .ConfigureAwait(continueOnCapturedContext: false);

            return guestInsertCount + regularInsertCount;
        }

        
        public async Task<int> AutoCheckInVolunteers()
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var volunteers = await db.Attendances
                .Where(predicate: a =>
                    a.AttendanceTypeId == (int) AttendanceTypeId.Volunteer
                    && a.CheckInDate == null)
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            volunteers.ForEach(action: v => v.CheckInDate = DateTime.UtcNow);
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public async Task<IImmutableList<PersistedLocation>> GetPersistedLocations()
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var locations = await db.Locations.Where(predicate: l => l.CheckInsLocationId.HasValue)
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            return locations.Select(selector: l
                    => new PersistedLocation(locationId: l.Id, checkInsLocationId: l.CheckInsLocationId!.Value))
                .ToImmutableList();
        }

        public async Task<int> UpdateLocations(IImmutableList<LocationUpdate> locationUpdates)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var locations = locationUpdates.Select(selector: MapLocation);
            await db.AddRangeAsync(entities: locations).ConfigureAwait(continueOnCapturedContext: false);
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task EnableUnknownLocationGroup()
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var locationGroups = await db.LocationGroups.Where(predicate: l => l.Id == (int) Shared.LocationGroup.Unknown)
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            locationGroups.ForEach(action: l => l.IsEnabled = true);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public async Task<int> AutoCheckoutEveryoneByEndOfDay()
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var attendances = await db.Attendances
                .Where(predicate: a =>
                    a.CheckInDate != null
                    && a.CheckOutDate == null
                    && a.CheckInDate < DateTime.Today)
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            attendances.ForEach(action: v
                => v.CheckOutDate = v.CheckInDate!.Value.Date.AddDays(value: 1).AddSeconds(value: -1));
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private static Location MapLocation(LocationUpdate locationUpdate)
        {
            return new()
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
                .Select(selector: c => c.Kid)
                .GroupBy(keySelector: p => p.PeopleId).Select(selector: p => p.First())
                .ToImmutableList();

            var existingPeople = await CommonRepository.GetKidsByPeopleIds(
                    db: db,
                    peopleIds: kids.Where(predicate: p => p.PeopleId.HasValue)
                        .Select(selector: p => p.PeopleId!.Value).ToImmutableList())
                .ConfigureAwait(continueOnCapturedContext: false);

            var peopleUpdates = kids.Where(
                    predicate: p => existingPeople.Select(selector: e => e.PeopleId).Contains(value: p.PeopleId))
                .ToImmutableList();

            var kidsInserts = kids.Except(second: peopleUpdates).ToImmutableList();
            var insertedPeople = await InsertPeople(db: db, peopleToInsert: kidsInserts)
                .ConfigureAwait(continueOnCapturedContext: false);

            var checkIns = regularPreCheckIns
                .Select(selector: c => MapToAttendance(checkInsUpdate: c,
                    people: existingPeople.Union(second: insertedPeople).ToImmutableList()))
                .ToImmutableList();
            await db.AddRangeAsync(entities: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private async Task<int> InsertGuests(IImmutableList<CheckInsUpdate> guests, DbContext db)
        {
            var existingCheckInsIds =
                await GetPersistedCheckInsIds(checkinsIds: guests.Select(selector: g => g.CheckInsId).ToImmutableList())
                    .ConfigureAwait(continueOnCapturedContext: false);
            var newGuests = guests.Where(predicate: g => !existingCheckInsIds.Contains(value: g.CheckInsId))
                .ToImmutableList();

            var guestAttendances = newGuests.Select(selector: MapGuestAttendance).ToImmutableList();

            await db.AddRangeAsync(entities: guestAttendances).ConfigureAwait(continueOnCapturedContext: false);
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private static async Task<List<Person>> InsertPeople(
            KidsTownContext db,
            IImmutableList<PeopleUpdate> peopleToInsert
        )
        {
            var people = peopleToInsert.Select(selector: MapPerson).ToImmutableList();
            await db.AddRangeAsync(entities: people).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);

            var insertedPeople = await db.People
                .Where(predicate: p => peopleToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return insertedPeople;
        }
        
        private static Attendance MapGuestAttendance(CheckInsUpdate guest)
        {
            var person = MapPerson(peopleUpdate: guest.Kid);
            var parent = MapParent(guest: guest);

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
            
            return MapToAttendance(checkInsUpdate: guest, people: ImmutableList.Create(item: person));
        }
        private static Person MapParent(CheckInsUpdate guest)
        {
            var names = guest.EmergencyContactName?.Split(separator: " ");

            var firstName = string.Empty;
            var lastName = string.Empty;

            if (names?.Length == 2)
            {
                firstName = names[0];
                lastName = names[1];
            } else if (guest.EmergencyContactName?.Length > 0)
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
            return new()
            {
                PeopleId = peopleUpdate.PeopleId,
                FirstName = peopleUpdate.FirstName,
                LastName = peopleUpdate.LastName,
                UpdateDate = DateTime.UtcNow
            };
        }
        
        private static Attendance MapToAttendance(CheckInsUpdate checkInsUpdate, IImmutableList<Person> people)
        {
            var person = people.Single(predicate: p => p.PeopleId == checkInsUpdate.PeopleId);

            return new Attendance
            {
                CheckInsId = checkInsUpdate.CheckInsId,
                LocationId = checkInsUpdate.LocationId,
                SecurityCode = checkInsUpdate.SecurityCode,
                InsertDate = checkInsUpdate.CreationDate,
                Person = person,
                AttendanceTypeId = MapAttendeeType(attendeeType: checkInsUpdate.AttendeeType)
            };
        }
        
        private static int MapAttendeeType(AttendeeType attendeeType)
        {
            return attendeeType switch
            {
                AttendeeType.Regular => 1,
                AttendeeType.Guest => 2,
                AttendeeType.Volunteer => 3,
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(attendeeType), actualValue: attendeeType,
                    message: null)
            };
        }

    }
}