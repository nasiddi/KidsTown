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
                    .Where(predicate: i
                        => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Kid.PeopleId.HasValue)
                    .Select(selector: i => i.Kid.PeopleId!.Value)
                    .Distinct()
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return peopleIds.ToImmutableList();
            }
        }

        public async Task UpdateKids(
            ImmutableList<PeopleUpdate> kids,
            ImmutableList<BackgroundTasks.Models.Family> families
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var existingKids = await GetKidsByPeopleIds(db: db,
                    peopleIds: kids.Select(selector: p => p.PeopleId!.Value)
                        .ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);

                await UpdateKids(db: db, people: existingKids, updates: kids, families)
                    .ConfigureAwait(continueOnCapturedContext: false);
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
                        && a.CheckInDate < DateTime.Today)
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                attendances.ForEach(action: v
                    => v.CheckOutDate = v.CheckInDate!.Value.Date.AddDays(value: 1).AddSeconds(value: -1));
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
                await PreCheckInRegulars(regularPreCheckIns: regularPreCheckIns, db: db)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<ImmutableList<PersistedLocation>> GetPersistedLocations()
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

        public async Task<ImmutableList<BackgroundTasks.Models.Family>> GetExistingFamilies(
            ImmutableList<long> householdIds
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var families = await db.Families.Where(f => householdIds.Contains(f.HouseholdId))
                    .ToListAsync().ConfigureAwait(false);

                return families.Select(MapFamily).ToImmutableList();
            }
        }

        public async Task<ImmutableList<BackgroundTasks.Models.Family>> InsertFamilies(
            ImmutableList<long> newHouseholdIds,
            ImmutableList<PeopleUpdate> peoples
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var families = newHouseholdIds.Select(h => MapFamily(h, peoples));
                await db.AddRangeAsync(families).ConfigureAwait(false);
                await db.SaveChangesAsync().ConfigureAwait(false);
                return await GetExistingFamilies(newHouseholdIds).ConfigureAwait(false);
            }
        }

        public async Task UpdateParents(IImmutableList<ParentUpdate> parentUpdates)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var peopleIds = parentUpdates.Select(p => p.PeopleId).ToImmutableList();
                var existingParents = await GetExistingParents(peopleIds, db);

                var updates = parentUpdates.Where(
                        p => existingParents.Select(e => e.PeopleId).Contains(p.PeopleId))
                    .ToImmutableList();
                existingParents.ForEach(e => UpdateParent(e, updates));

                var newEntries = parentUpdates.Except(updates).ToImmutableList();
                var newParents = newEntries.Select(MapParent);
                await db.AddRangeAsync(newParents);
                await db.SaveChangesAsync();
            }
        }

        private static Adult MapParent(ParentUpdate parent)
        {
            return new()
            {
                PeopleId = parent.PeopleId,
                FamilyId = parent.FamilyId,
                FirstName = parent.FirstName,
                LastName = parent.LastName,
                PhoneNumber = parent.PhoneNumber,
                UpdateDate = DateTime.UtcNow,
            };
        }

        private static void UpdateParent(Adult parent, ImmutableList<ParentUpdate> updates)
        {
            var update = updates.Single(u => u.PeopleId == parent.PeopleId);

            parent.FamilyId = update.FamilyId;

            if (update.FirstName.Length < 0)
            {
                parent.FirstName = update.FirstName;
            }
            
            if (update.LastName.Length < 0)
            {
                parent.LastName = update.LastName;
            }
            
            if (update.PhoneNumber.Length < 0)
            {
                parent.PhoneNumber = update.PhoneNumber;
            }
            
            parent.UpdateDate = DateTime.UtcNow;
        }


        private static async Task<ImmutableList<Adult>> GetExistingParents(
            IImmutableList<long> peopleIds,
            KidsTownContext db
        )
        {
            var parents = await db.Adults.Where(p => peopleIds.Contains(p.PeopleId)).ToListAsync()
                .ConfigureAwait(false);
            return parents.ToImmutableList();
        }

        private static Family MapFamily(long householdId, ImmutableList<PeopleUpdate> peoples)
        {
            var name = peoples.First(p => p.HouseholdId == householdId).HouseholdName;

            return new Family
            {
                HouseholdId = householdId,
                Name = name,
            };
        }

        private static BackgroundTasks.Models.Family MapFamily(Family family)
        {
            return new(
                family.Id,
                family.HouseholdId);
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

        private static async Task PreCheckInRegulars(
            ImmutableList<CheckInsUpdate> regularPreCheckIns,
            KidsTownContext db
        )
        {
            var kids = regularPreCheckIns
                .Select(selector: c => c.Kid)
                .GroupBy(keySelector: p => p.PeopleId).Select(selector: p => p.First())
                .ToImmutableList();

            var existingKids = await GetKidsByPeopleIds(
                    db: db,
                    peopleIds: kids.Where(predicate: p => p.PeopleId.HasValue)
                        .Select(selector: p => p.PeopleId!.Value).ToImmutableList())
                .ConfigureAwait(continueOnCapturedContext: false);

            var peopleUpdates = kids.Where(
                    predicate: p => existingKids.Select(selector: e => e.PeopleId).Contains(value: p.PeopleId))
                .ToImmutableList();
            await UpdateKids(
                    db: db,
                    people: existingKids,
                    updates: peopleUpdates,
                    ImmutableList<BackgroundTasks.Models.Family>.Empty)
                .ConfigureAwait(continueOnCapturedContext: false);

            var kidsInserts = kids.Except(second: peopleUpdates).ToImmutableList();
            var insertKids = await InsertKids(db: db, kidsToInsert: kidsInserts)
                .ConfigureAwait(continueOnCapturedContext: false);

            var checkIns = regularPreCheckIns
                .Select(selector: c => MapToAttendance(checkInsUpdate: c,
                    kids: existingKids.Union(second: insertKids).ToImmutableList()))
                .ToImmutableList();
            await db.AddRangeAsync(entities: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task PreCheckInGuests(ImmutableList<CheckInsUpdate> guests, DbContext db)
        {
            var existingCheckInsIds =
                await GetExistingCheckInsIds(checkinsIds: guests.Select(selector: g => g.CheckInsId).ToImmutableList())
                    .ConfigureAwait(continueOnCapturedContext: false);
            var newGuests = guests.Where(predicate: g => !existingCheckInsIds.Contains(value: g.CheckInsId))
                .ToImmutableList();

            var guestAttendances = newGuests.Select(selector: MapGuestAttendance).ToImmutableList();

            await db.AddRangeAsync(entities: guestAttendances).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static Attendance MapGuestAttendance(CheckInsUpdate guest)
        {
            var kid = MapKid(peopleUpdate: guest.Kid);
            return MapToAttendance(checkInsUpdate: guest, kids: ImmutableList.Create(item: kid));
        }

        private static async Task<List<Kid>> InsertKids(
            KidsTownContext db,
            IImmutableList<PeopleUpdate> kidsToInsert
        )
        {
            var kids = kidsToInsert.Select(selector: MapKid).ToImmutableList();
            await db.AddRangeAsync(entities: kids).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);

            var insertedPeople = await db.Kids
                .Where(predicate: p => kidsToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return insertedPeople;
        }

        private static Kid MapKid(PeopleUpdate peopleUpdate)
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

        private static async Task UpdateKids(
            DbContext db,
            List<Kid> people,
            ImmutableList<PeopleUpdate> updates,
            ImmutableList<BackgroundTasks.Models.Family> families
        )
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
                p.FamilyId = families.SingleOrDefault(f => f.HouseholdId == update.HouseholdId)?.FamilyId;
            });

            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static async Task<List<Kid>> GetKidsByPeopleIds(
            KidsTownContext db,
            IImmutableList<long> peopleIds
        )
        {
            var kids = await db.Kids
                .Where(predicate: p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            return kids;
        }

        private static Attendance MapToAttendance(CheckInsUpdate checkInsUpdate, ImmutableList<Kid> kids)
        {
            var kid = kids.SingleOrDefault(predicate: p => p.PeopleId == checkInsUpdate.PeopleId);

            return new Attendance
            {
                CheckInsId = checkInsUpdate.CheckInsId,
                LocationId = checkInsUpdate.LocationId,
                SecurityCode = checkInsUpdate.SecurityCode,
                InsertDate = checkInsUpdate.CreationDate,
                Kid = kid,
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