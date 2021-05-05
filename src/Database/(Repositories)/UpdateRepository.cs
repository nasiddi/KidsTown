using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.Shared;
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

        public async Task<ImmutableList<TypedAttendee>> GetCurrentPeopleIds(int daysLookBack)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var typedAttendees = await db.Attendances
                    .Where(predicate: i
                        => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Person.PeopleId.HasValue)
                    .Select(selector: a => new TypedAttendee(a.Person.PeopleId!.Value, (AttendanceTypeId)a.AttendanceTypeId))
                    .Distinct()
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return typedAttendees.ToImmutableList();
            }
        }

        public async Task<int> UpdateKids(
            IImmutableList<PeopleUpdate> kids,
            IImmutableList<BackgroundTasks.Adult.Family> families
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var existingKids = await GetKidsByPeopleIds(db: db,
                    peopleIds: kids.Select(selector: p => p.PeopleId!.Value)
                        .ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);

                return await UpdateKids(db: db, people: existingKids, updates: kids, families: families)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<int> AutoCheckInVolunteers()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var volunteers = await db.Attendances
                    .Where(predicate: a =>
                        a.AttendanceTypeId == (int) AttendanceTypeId.Volunteer
                        && a.CheckInDate == null)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                volunteers.ForEach(action: v => v.CheckInDate = DateTime.UtcNow);
                return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<int> AutoCheckoutEveryoneByEndOfDay()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
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
        }

        public async Task<int> InsertPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var guests = preCheckIns.Where(predicate: c => c.PeopleId == null).ToImmutableList();
                var guestInsertCount = await PreCheckInGuests(guests: guests, db: db).ConfigureAwait(continueOnCapturedContext: false);

                var regularPreCheckIns = preCheckIns.Except(second: guests).ToImmutableList();
                var regularInsertCount = await PreCheckInRegulars(regularPreCheckIns: regularPreCheckIns, db: db)
                    .ConfigureAwait(continueOnCapturedContext: false);

                return guestInsertCount + regularInsertCount;
            }
        }

        public async Task<IImmutableList<PersistedLocation>> GetPersistedLocations()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var locations = await db.Locations.Where(predicate: l => l.CheckInsLocationId.HasValue)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                return locations.Select(selector: l
                        => new PersistedLocation(locationId: l.Id, checkInsLocationId: l.CheckInsLocationId!.Value))
                    .ToImmutableList();
            }
        }

        public async Task<int> UpdateLocations(IImmutableList<LocationUpdate> locationUpdates)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var locations = locationUpdates.Select(selector: MapLocation);
                await db.AddRangeAsync(entities: locations).ConfigureAwait(continueOnCapturedContext: false);
                return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task EnableUnknownLocationGroup()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var locationGroups = await db.LocationGroups.Where(predicate: l => l.Id == (int) Shared.LocationGroup.Unknown)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                locationGroups.ForEach(action: l => l.IsEnabled = true);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task LogTaskRun(bool success, int updateCount, string environment, string taskName)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var taskExecution = new TaskExecution
                {
                    InsertDate = DateTime.UtcNow,
                    IsSuccess = success,
                    UpdateCount = updateCount,
                    Environment = environment,
                    TaskName = taskName
                };

                var taskExecutionCount = await db.TaskExecutions.Where(predicate: t => t.TaskName == taskName).CountAsync();

                if (taskExecutionCount >= 100)
                {
                    var toBeDeleted = taskExecutionCount - 99;

                    var taskExecutionsToDelete = await db.TaskExecutions.OrderBy(keySelector: t => t.Id)
                        .Where(predicate: t => t.TaskName == taskName)
                        .Take(count: toBeDeleted).ToListAsync()
                        .ConfigureAwait(continueOnCapturedContext: false);

                    db.RemoveRange(entities: taskExecutionsToDelete);
                }

                await db.AddAsync(entity: taskExecution).ConfigureAwait(continueOnCapturedContext: false);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> GetExistingFamilies(
            IImmutableList<long> householdIds
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var families = await db.Families.Where(predicate: f => householdIds.Contains(f.HouseholdId))
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return families.Select(selector: MapFamily).ToImmutableList();
            }
        }

        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> InsertFamilies(
            IImmutableList<long> newHouseholdIds,
            IImmutableList<PeopleUpdate> peoples
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var families = newHouseholdIds.Select(selector: h => MapFamily(householdId: h, peoples: peoples));
                await db.AddRangeAsync(entities: families).ConfigureAwait(continueOnCapturedContext: false);
                await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
                return await GetExistingFamilies(householdIds: newHouseholdIds).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<int> UpdateParents(IImmutableList<AdultUpdate> parentUpdates)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var peopleIds = parentUpdates.Select(selector: p => p.PeopleId).ToImmutableList();
                var existingParents = await GetExistingParents(peopleIds: peopleIds, db: db).ConfigureAwait(continueOnCapturedContext: false);

                var updates = parentUpdates.Where(
                        predicate: p => existingParents.Select(selector: e => e.PeopleId).Contains(value: p.PeopleId))
                    .ToImmutableList();
                
                foreach (var existingParent in existingParents)
                {
                    UpdateParent(person: existingParent, updates: updates);
                }
                
                var newEntries = parentUpdates.Except(second: updates).ToImmutableList();
                var newParents = newEntries.Select(selector: MapParent);

                await SetFamilyUpdateDate(db: db, updates: newEntries);
                
                await db.AddRangeAsync(entities: newParents);
                return await db.SaveChangesAsync();
            }
        }

        private static async Task SetFamilyUpdateDate(KidsTownContext db, ImmutableList<AdultUpdate> updates)
        {
            var families = await db.Families
                .Where(predicate: f => updates.Select(e => e.FamilyId).Contains(f.Id))
                .ToListAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            
            var updateDate = DateTime.UtcNow;
            families.ForEach(action: f => f.UpdateDate = updateDate);
        }

        private static Person MapParent(AdultUpdate adultUpdate)
        {
            var adult = new Adult
            {
                PhoneNumber = adultUpdate.PhoneNumber
            };

            return new Person
            {
                PeopleId = adultUpdate.PeopleId,
                FamilyId = adultUpdate.FamilyId,
                FirstName = adultUpdate.FirstName,
                LastName = adultUpdate.LastName,
                UpdateDate = DateTime.UtcNow,
                Adult = adult
            };
        }

        private static void UpdateParent(Person person, IImmutableList<AdultUpdate> updates)
        {
            var update = updates.Single(predicate: u => u.PeopleId == person.PeopleId);

            person.FamilyId = update.FamilyId;

            if (update.FirstName.Length > 0)
            {
                person.FirstName = update.FirstName;
            }
            
            if (update.LastName.Length > 0)
            {
                person.LastName = update.LastName;
            }
            
            if (update.PhoneNumber.Length > 0)
            {
                person.Adult ??= new Adult();
                person.Adult.PhoneNumber = update.PhoneNumber;
            }

            var updateDate = DateTime.UtcNow;
            person.UpdateDate = updateDate;
            person.Family.UpdateDate = updateDate;
        }


        private static async Task<IImmutableList<Person>> GetExistingParents(
            IImmutableList<long> peopleIds,
            KidsTownContext db
        )
        {
            var parents = await db.People
                .Where(predicate: p => p.PeopleId != null && peopleIds.Contains(p.PeopleId.Value))
                .Include(navigationPropertyPath: p => p.Adult)
                .Include(navigationPropertyPath: p => p.Family)
                .ToListAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            return parents.ToImmutableList();
        }

        private static Family MapFamily(long householdId, IImmutableList<PeopleUpdate> peoples)
        {
            var name = peoples.First(predicate: p => p.HouseholdId == householdId).HouseholdName;

            return new Family
            {
                HouseholdId = householdId,
                Name = name,
                UpdateDate = DateTime.UnixEpoch
            };
        }

        private static BackgroundTasks.Adult.Family MapFamily(Family family)
        {
            return new(
                familyId: family.Id,
                householdId: family.HouseholdId);
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

        private static async Task<int> PreCheckInRegulars(
            IImmutableList<CheckInsUpdate> regularPreCheckIns,
            KidsTownContext db
        )
        {
            var kids = regularPreCheckIns
                .Select(selector: c => c.Kid)
                .GroupBy(keySelector: p => p.PeopleId).Select(selector: p => p.First())
                .ToImmutableList();

            var existingPeople = await GetKidsByPeopleIds(
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

        private async Task<int> PreCheckInGuests(IImmutableList<CheckInsUpdate> guests, DbContext db)
        {
            var existingCheckInsIds =
                await GetExistingCheckInsIds(checkinsIds: guests.Select(selector: g => g.CheckInsId).ToImmutableList())
                    .ConfigureAwait(continueOnCapturedContext: false);
            var newGuests = guests.Where(predicate: g => !existingCheckInsIds.Contains(value: g.CheckInsId))
                .ToImmutableList();

            var guestAttendances = newGuests.Select(selector: MapGuestAttendance).ToImmutableList();

            await db.AddRangeAsync(entities: guestAttendances).ConfigureAwait(continueOnCapturedContext: false);
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static Attendance MapGuestAttendance(CheckInsUpdate guest)
        {
            var person = MapPerson(peopleUpdate: guest.Kid);
            return MapToAttendance(checkInsUpdate: guest, people: ImmutableList.Create(item: person));
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

        private static async Task<int> UpdateKids(
            DbContext db,
            List<Person> people,
            IImmutableList<PeopleUpdate> updates,
            IImmutableList<BackgroundTasks.Adult.Family> families
        )
        {
            var updatesByPeopleId = updates.Where(predicate: u => u.PeopleId.HasValue)
                .ToImmutableDictionary(keySelector: k => k.PeopleId!, elementSelector: v => v);

            var updateDate = DateTime.UtcNow;
            
            people.ForEach(action: p =>
            {
                var update = updatesByPeopleId[key: p.PeopleId!];

                p.Kid ??= new Kid();

                p.Kid.MayLeaveAlone = update.MayLeaveAlone;
                p.Kid.HasPeopleWithoutPickupPermission = update.HasPeopleWithoutPickupPermission;

                p.FirstName = update.FirstName;
                p.LastName = update.LastName;
                p.FamilyId = families.SingleOrDefault(predicate: f => f.HouseholdId == update.HouseholdId)?.FamilyId;
                p.UpdateDate = updateDate;
            });
            
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static async Task<List<Person>> GetKidsByPeopleIds(
            KidsTownContext db,
            IImmutableList<long> peopleIds
        )
        {
            var people = await db.People
                .Where(predicate: p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .Include(navigationPropertyPath: p => p.Kid)
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            return people;
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