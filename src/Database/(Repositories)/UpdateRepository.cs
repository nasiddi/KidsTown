using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Models;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.KidsTown.Models;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToUsingDeclaration

namespace KidsTown.Database
{
    public class UpdateRepository : IUpdateRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IImmutableList<long>> GetExistingCheckInsIds(IImmutableList<long> checkinsIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var existingCheckInsIds = await db.Attendances.Where(predicate: i => checkinsIds.Contains(i.CheckInsId))
                    .Select(selector: i => i.CheckInsId)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return existingCheckInsIds.ToImmutableList();
            }
        }

        public async Task<ImmutableList<long>> GetCurrentPeopleIds(int daysLookBack)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var peopleIds = await db.Attendances
                    .Where(predicate: i => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Person.PeopleId.HasValue)
                    .Select(selector: i => i.Person.PeopleId!.Value)
                    .Distinct()
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return peopleIds.ToImmutableList();
            }
        }

        public async Task UpdatePersons(ImmutableList<PeopleUpdate> peoples)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var existingPersons =
                    await GetPersonsByPeopleIds(db: db, peopleIds: peoples.Select(selector: p => p.PeopleId!.Value).ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);

                await UpdatePersons(db: db, people: existingPersons, updates: peoples).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task AutoCheckInVolunteers()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var volunteers = await db.Attendances
                    .Where(predicate: a => 
                        a.AttendanceTypeId == (int) AttendanceTypes.Volunteer
                        && a.CheckInDate == null)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                
                volunteers.ForEach(action: v => v.CheckInDate = DateTime.UtcNow);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task AutoCheckoutEveryoneByEndOfDay()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var attendances = await db.Attendances
                    .Where(predicate: a => 
                        a.CheckInDate != null
                        && a.CheckOutDate == null
                        && a. CheckInDate < DateTime.Today)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                
                attendances.ForEach(action: v => v.CheckOutDate = v.CheckInDate!.Value.Date.AddDays(value: 1).AddSeconds(value: -1));
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task InsertPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var guests = preCheckIns.Where(predicate: c => c.PeopleId == null).ToImmutableList();
                await PreCheckInGuests(guests: guests, db: db).ConfigureAwait(continueOnCapturedContext: false);

                var regularPreCheckIns = preCheckIns.Except(second: guests).ToImmutableList();
                await PreCheckInRegulars(regularPreCheckIns: regularPreCheckIns, db: db).ConfigureAwait(continueOnCapturedContext: false);
            }
        }
        
        public async Task<ImmutableList<PersistedLocation>> GetPersistedLocations()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var locations = await db.Locations.Where(predicate: l => l.CheckInsLocationId.HasValue)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                return locations.Select(selector: l => new PersistedLocation(locationId: l.Id, checkInsLocationId: l.CheckInsLocationId!.Value)).ToImmutableList();
            }
        }

        public async Task UpdateLocations(ImmutableList<LocationUpdate> locationUpdates)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var locations = locationUpdates.Select(selector: MapLocation);
                await db.AddRangeAsync(entities: locations).ConfigureAwait(continueOnCapturedContext: false);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task EnableUnknownLocationGroup()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var locationGroups = await db.LocationGroups.Where(predicate: l => l.Id == (int) LocationGroups.Unknown)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                
                locationGroups.ForEach(action: l => l.IsEnabled = true);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task LogTaskRun(bool success, int updateCount, string environment)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var taskExecution = new TaskExecution
                {
                    InsertDate = DateTime.UtcNow,
                    IsSuccess = success,
                    UpdateCount = updateCount,
                    Environment = environment
                };

                var taskExecutionCount = await db.TaskExecutions.CountAsync();

                if (taskExecutionCount >= 500)
                {
                    var toBeDeleted = taskExecutionCount - 499;

                    var taskExecutionsToDelete = await db.TaskExecutions.OrderBy(keySelector: t => t.Id)
                        .Take(count: toBeDeleted).ToListAsync()
                        .ConfigureAwait(continueOnCapturedContext: false);
                    
                    db.RemoveRange(entities: taskExecutionsToDelete);
                }

                await db.AddAsync(entity: taskExecution).ConfigureAwait(continueOnCapturedContext: false);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
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

        private static async Task PreCheckInRegulars(ImmutableList<CheckInsUpdate> regularPreCheckIns, KidsTownContext db)
        {
            var persons = regularPreCheckIns
                .Select(selector: c => c.Person)
                .GroupBy(keySelector: p => p.PeopleId).Select(selector: p => p.First())
                .ToImmutableList();

            var existingPersons = await GetPersonsByPeopleIds(
                db: db,
                peopleIds: persons.Where(predicate: p => p.PeopleId.HasValue)
                    .Select(selector: p => p.PeopleId!.Value).ToImmutableList())
                .ConfigureAwait(continueOnCapturedContext: false);

            var personUpdates = persons.Where(
                    predicate: p => existingPersons.Select(selector: e => e.PeopleId).Contains(value: p.PeopleId))
                .ToImmutableList();
            await UpdatePersons(db: db, people: existingPersons, updates: personUpdates).ConfigureAwait(continueOnCapturedContext: false);
            
            var personInserts = persons.Except(second: personUpdates).ToImmutableList();
            var insertedPersons = await InsertPersons(db: db, personToInsert: personInserts).ConfigureAwait(continueOnCapturedContext: false);

            var checkIns = regularPreCheckIns
                .Select(selector: c => MapToAttendance(checkInsUpdate: c, persons: existingPersons.Union(second: insertedPersons).ToImmutableList()))
                .ToImmutableList();
            await db.AddRangeAsync(entities: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task PreCheckInGuests(ImmutableList<CheckInsUpdate> guests, DbContext db)
        {
            var existingCheckInsIds = await GetExistingCheckInsIds(checkinsIds: guests.Select(selector: g => g.CheckInsId).ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);
            var newGuests = guests.Where(predicate: g => !existingCheckInsIds.Contains(value: g.CheckInsId)).ToImmutableList();

            var guestAttendances = newGuests.Select(selector: MapGuestAttendance).ToImmutableList();
            
            await db.AddRangeAsync(entities: guestAttendances).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static Attendance MapGuestAttendance(CheckInsUpdate guest)
        {
            var person = MapPerson(peopleUpdate: guest.Person);
            return MapToAttendance(checkInsUpdate: guest, persons: ImmutableList.Create(item: person));
        }

        private static async Task<List<Person>> InsertPersons(KidsTownContext db,
            IImmutableList<PeopleUpdate> personToInsert)
        {
            var persons = personToInsert.Select(selector: MapPerson).ToImmutableList();
            await db.AddRangeAsync(entities: persons).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);

            var insertedPeople = await db.People.Where(predicate: p => personToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

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
            DbContext db,
            List<Person> people,
            ImmutableList<PeopleUpdate> updates)
        {
            var updatesByPeopleId = updates.Where(predicate: u => u.PeopleId.HasValue)
                .ToImmutableDictionary(keySelector: k => k.PeopleId!, elementSelector: v => v);

            people.ForEach(action: p =>
            {
                var update = updatesByPeopleId[key: p.PeopleId!];

                p.FistName = update.FirstName;
                p.LastName = update.LastName;
                p.MayLeaveAlone = update.MayLeaveAlone;
                p.HasPeopleWithoutPickupPermission = update.HasPeopleWithoutPickupPermission;
            });

            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static async Task<List<Person>> GetPersonsByPeopleIds(
            KidsTownContext db,
            IImmutableList<long> peopleIds)
        {
            var people = await db.People.Where(predicate: p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            return people;
        }

        private static Attendance MapToAttendance(CheckInsUpdate checkInsUpdate, ImmutableList<Person> persons)
        {
            var person = persons.SingleOrDefault(predicate: p => p.PeopleId == checkInsUpdate.PeopleId);

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
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(attendeeType), actualValue: attendeeType, message: null)
            };
        }
    }
}