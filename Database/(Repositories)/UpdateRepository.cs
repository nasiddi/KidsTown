using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.Update;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToUsingDeclaration

namespace ChekInsExtension.Database
{
    public class UpdateRepository : IUpdateRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IImmutableList<long>> GetExistingCheckInIds(IImmutableList<long> checkinIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var existingCheckInIds = await db.Attendances.Where(i => checkinIds.Contains(i.CheckInId))
                    .Select(i => i.CheckInId)
                    .ToListAsync().ConfigureAwait(false);

                return existingCheckInIds.ToImmutableList();
            }
        }

        public async Task<ImmutableList<long>> GetCurrentPeopleIds(int daysLookBack)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var peopleIds = await db.Attendances
                    .Where(i => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Person.PeopleId.HasValue)
                    .Select(i => i.Person.PeopleId!.Value)
                    .Distinct()
                    .ToListAsync().ConfigureAwait(false);

                return peopleIds.ToImmutableList();
            }
        }

        public async Task UpdatePersons(ImmutableList<PeopleUpdate> peoples)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var existingPersons =
                    await GetPersonsByPeopleIds(db, peoples.Select(p => p.PeopleId!.Value).ToImmutableList()).ConfigureAwait(false);

                await UpdatePersons(db, existingPersons, peoples).ConfigureAwait(false);
            }
        }

        public async Task AutoCheckInVolunteers()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var volunteers = await db.Attendances
                    .Where(a => 
                        a.AttendanceTypeId == (int) AttendanceTypes.Volunteer
                        && a.CheckInDate == null)
                    .ToListAsync().ConfigureAwait(false);
                
                volunteers.ForEach(v => v.CheckInDate = DateTime.UtcNow);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task AutoCheckoutEveryoneByEndOfDay()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var attendances = await db.Attendances
                    .Where(a => 
                        a.CheckInDate != null
                        && a.CheckOutDate == null
                        && a. CheckInDate < DateTime.Today)
                    .ToListAsync().ConfigureAwait(false);
                
                attendances.ForEach(v => v.CheckOutDate = v.CheckInDate!.Value.Date.AddDays(1).AddSeconds(-1));
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task InsertPreCheckIns(IImmutableList<CheckInUpdate> preCheckIns)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var guests = preCheckIns.Where(c => c.PeopleId == null).ToImmutableList();
                await PreCheckInGuests(guests, db).ConfigureAwait(false);

                var regularPreCheckIns = preCheckIns.Except(guests).ToImmutableList();
                await PreCheckInRegulars(regularPreCheckIns, db).ConfigureAwait(false);
            }
        }
        
        public async Task<ImmutableList<PersistedLocation>> GetPersistedLocations()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.Locations.Where(l => l.CheckInsLocationId.HasValue)
                    .ToListAsync().ConfigureAwait(false);
                return locations.Select(l => new PersistedLocation(l.Id, l.CheckInsLocationId!.Value)).ToImmutableList();
            }
        }

        public async Task UpdateLocations(ImmutableList<LocationUpdate> locationUpdates)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = locationUpdates.Select(MapLocation);
                await db.AddRangeAsync(locations).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task EnableUnknownLocationGroup()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var locationGroups = await db.LocationGroups.Where(l => l.Id == (int) LocationGroups.Unknown)
                    .ToListAsync().ConfigureAwait(false);
                
                locationGroups.ForEach(l => l.IsEnabled = true);
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
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

        private static async Task PreCheckInRegulars(ImmutableList<CheckInUpdate> regularPreCheckIns, CheckInsExtensionContext db)
        {
            var persons = regularPreCheckIns
                .Select(c => c.Person)
                .GroupBy(p => p.PeopleId).Select(p => p.First())
                .ToImmutableList();

            var existingPersons = await GetPersonsByPeopleIds(
                db,
                persons.Where(p => p.PeopleId.HasValue)
                    .Select(p => p.PeopleId!.Value).ToImmutableList())
                .ConfigureAwait(false);

            var personUpdates = persons.Where(
                    p => existingPersons.Select(e => e.PeopleId).Contains(p.PeopleId))
                .ToImmutableList();
            await UpdatePersons(db, existingPersons, personUpdates).ConfigureAwait(false);
            
            var personInserts = persons.Except(personUpdates).ToImmutableList();
            var insertedPersons = await InsertPersons(db, personInserts).ConfigureAwait(false);

            var checkIns = regularPreCheckIns
                .Select(c => MapToAttendance(c, existingPersons.Union(insertedPersons).ToImmutableList()))
                .ToImmutableList();
            await db.AddRangeAsync(checkIns).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task PreCheckInGuests(ImmutableList<CheckInUpdate> guests, CheckInsExtensionContext db)
        {
            var existingCheckInIds = await GetExistingCheckInIds(guests.Select(g => g.CheckInId).ToImmutableList()).ConfigureAwait(false);
            var newGuests = guests.Where(g => !existingCheckInIds.Contains(g.CheckInId)).ToImmutableList();

            var guestAttendances = newGuests.Select(MapGuestAttendance).ToImmutableList();
            
            await db.AddRangeAsync(guestAttendances).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        private static Attendance MapGuestAttendance(CheckInUpdate guest)
        {
            var person = MapPerson(guest.Person);
            return MapToAttendance(guest, ImmutableList.Create(person));
        }

        private static async Task<List<Person>> InsertPersons(CheckInsExtensionContext db,
            IImmutableList<PeopleUpdate> personToInsert)
        {
            var persons = personToInsert.Select(MapPerson).ToImmutableList();
            await db.AddRangeAsync(persons).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);

            var insertedPeople = await db.People.Where(p => personToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync().ConfigureAwait(false);

            return insertedPeople;
        }

        private static Person MapPerson(PeopleUpdate peopleUpdate)
        {
            return new()
            {
                PeopleId = peopleUpdate.PeopleId,
                FistName = peopleUpdate.FirstName,
                LastName = peopleUpdate.LastName,
                MayLeaveAlone = peopleUpdate.MayLeaveAlone,
                HasPeopleWithoutPickupPermission = peopleUpdate.HasPeopleWithoutPickupPermission
            };
        }

        private static async Task UpdatePersons(
            CheckInsExtensionContext db,
            List<Person> people,
            ImmutableList<PeopleUpdate> updates)
        {
            var updatesByPeopleId = updates.Where(u => u.PeopleId.HasValue)
                .ToImmutableDictionary(k => k.PeopleId!, v => v);

            people.ForEach(p =>
            {
                var update = updatesByPeopleId[p.PeopleId!];

                p.FistName = update.FirstName;
                p.LastName = update.LastName;
                p.MayLeaveAlone = update.MayLeaveAlone;
                p.HasPeopleWithoutPickupPermission = update.HasPeopleWithoutPickupPermission;
            });

            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        private static async Task<List<Person>> GetPersonsByPeopleIds(
            CheckInsExtensionContext db,
            IImmutableList<long> peopleIds)
        {
            var people = await db.People.Where(p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .ToListAsync().ConfigureAwait(false);
            return people;
        }

        private static Attendance MapToAttendance(CheckInUpdate checkInUpdate, ImmutableList<Person> persons)
        {
            var person = persons.SingleOrDefault(p => p.PeopleId == checkInUpdate.PeopleId);

            return new Attendance
            {
                CheckInId = checkInUpdate.CheckInId,
                LocationId = checkInUpdate.LocationId,
                SecurityCode = checkInUpdate.SecurityCode,
                InsertDate = checkInUpdate.CreationDate,
                Person = person,
                AttendanceTypeId = MapAttendeeType(checkInUpdate.AttendeeType)
            };
        }

        private static int MapAttendeeType(AttendeeType attendeeType)
        {
            return attendeeType switch
            {
                AttendeeType.Regular => 1,
                AttendeeType.Guest => 2,
                AttendeeType.Volunteer => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(attendeeType), attendeeType, null)
            };
        }
    }
}