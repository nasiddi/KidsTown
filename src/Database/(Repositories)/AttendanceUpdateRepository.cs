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
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            var existingCheckInsIds = await db.Attendances.Where(i => checkinsIds.Contains(i.CheckInsId))
                .Select(i => i.CheckInsId)
                .ToListAsync().ConfigureAwait(false);

            return existingCheckInsIds.ToImmutableList();
        }
        
        public async Task<int> InsertAttendances(IImmutableList<CheckInsUpdate> checkInsUpdates)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            
            var guests = checkInsUpdates.Where(c => c.PeopleId == null).ToImmutableList();
            var guestInsertCount = await InsertGuests(guests: guests, db: db).ConfigureAwait(false);

            var regularPreCheckIns = checkInsUpdates.Except(guests).ToImmutableList();
            var regularInsertCount = await InsertRegularsAndVolunteers(regularPreCheckIns: regularPreCheckIns, db: db)
                .ConfigureAwait(false);

            return guestInsertCount + regularInsertCount;
        }

        
        public async Task<int> AutoCheckInVolunteers()
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var volunteers = await db.Attendances
                .Where(a =>
                    a.AttendanceTypeId == (int) AttendanceTypeId.Volunteer
                    && a.CheckInDate == null)
                .ToListAsync().ConfigureAwait(false);

            volunteers.ForEach(v => v.CheckInDate = DateTime.UtcNow);
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }
        
        public async Task<IImmutableList<PersistedLocation>> GetPersistedLocations()
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var locations = await db.Locations.Where(l => l.CheckInsLocationId.HasValue)
                .ToListAsync().ConfigureAwait(false);
            return locations.Select(l
                    => new PersistedLocation(locationId: l.Id, checkInsLocationId: l.CheckInsLocationId!.Value))
                .ToImmutableList();
        }

        public async Task<int> UpdateLocations(IImmutableList<LocationUpdate> locationUpdates)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var locations = locationUpdates.Select(MapLocation);
            await db.AddRangeAsync(locations).ConfigureAwait(false);
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task EnableUnknownLocationGroup()
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var locationGroups = await db.LocationGroups.Where(l => l.Id == (int) Shared.LocationGroup.Unknown)
                .ToListAsync().ConfigureAwait(false);

            locationGroups.ForEach(l => l.IsEnabled = true);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
        
        public async Task<int> AutoCheckoutEveryoneByEndOfDay()
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var attendances = await db.Attendances
                .Where(a =>
                    a.CheckInDate != null
                    && a.CheckOutDate == null
                    && a.CheckInDate < DateTime.Today)
                .ToListAsync().ConfigureAwait(false);

            attendances.ForEach(v
                => v.CheckOutDate = v.CheckInDate!.Value.Date.AddDays(1).AddSeconds(-1));
            return await db.SaveChangesAsync().ConfigureAwait(false);
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
                .Select(c => c.Kid)
                .GroupBy(p => p.PeopleId).Select(p => p.First())
                .ToImmutableList();

            var existingPeople = await CommonRepository.GetKidsByPeopleIds(
                    db: db,
                    peopleIds: kids.Where(p => p.PeopleId.HasValue)
                        .Select(p => p.PeopleId!.Value).ToImmutableList())
                .ConfigureAwait(false);

            var peopleUpdates = kids.Where(
                    p => existingPeople.Select(e => e.PeopleId).Contains(p.PeopleId))
                .ToImmutableList();

            var kidsInserts = kids.Except(peopleUpdates).ToImmutableList();
            var insertedPeople = await InsertPeople(db: db, peopleToInsert: kidsInserts)
                .ConfigureAwait(false);

            var checkIns = regularPreCheckIns
                .Select(c => MapToAttendance(checkInsUpdate: c,
                    people: existingPeople.Union(insertedPeople).ToImmutableList()))
                .ToImmutableList();
            await db.AddRangeAsync(checkIns).ConfigureAwait(false);
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }
        
        private async Task<int> InsertGuests(IImmutableList<CheckInsUpdate> guests, DbContext db)
        {
            var existingCheckInsIds =
                await GetPersistedCheckInsIds(guests.Select(g => g.CheckInsId).ToImmutableList())
                    .ConfigureAwait(false);
            var newGuests = guests.Where(g => !existingCheckInsIds.Contains(g.CheckInsId))
                .ToImmutableList();

            var guestAttendances = newGuests.Select(MapGuestAttendance).ToImmutableList();

            await db.AddRangeAsync(guestAttendances).ConfigureAwait(false);
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }
        
        private static async Task<List<Person>> InsertPeople(
            KidsTownContext db,
            IImmutableList<PeopleUpdate> peopleToInsert
        )
        {
            var people = peopleToInsert.Select(MapPerson).ToImmutableList();
            await db.AddRangeAsync(people).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);

            var insertedPeople = await db.People
                .Where(p => peopleToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync().ConfigureAwait(false);

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
            
            return MapToAttendance(checkInsUpdate: guest, people: ImmutableList.Create(person));
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
            } else if (guest.EmergencyContactName?.Length > 0)
            {
                firstName = guest.EmergencyContactName;
            }

            var adult = new Adult
            {
                PhoneNumber = guest.EmergencyContactNumber,
                IsPrimaryContact = false,
            };
            
            return new Person
            {
                FirstName = firstName,
                LastName = lastName,
                UpdateDate = DateTime.UtcNow,
                Adult = adult,
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
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(attendeeType), actualValue: attendeeType,
                    message: null)
            };
        }

    }
}