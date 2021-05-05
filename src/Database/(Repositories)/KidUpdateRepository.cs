using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.Kid;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database
{
    public class KidUpdateRepository : IKidUpdateRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KidUpdateRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<ImmutableList<TypedAttendee>> GetKidsToUpdate(int daysLookBack, int take)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var typedAttendees = await db.Attendances
                .Include(navigationPropertyPath: a => a.Person)
                .Where(predicate: i
                    => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Person.PeopleId.HasValue)
                .OrderBy(keySelector: a => a.Person.UpdateDate)
                .ThenBy(keySelector: a => a.Person.Id)
                .Take(count: take)
                .Select(selector: a => new TypedAttendee(a.Person.PeopleId!.Value, (AttendanceTypeId)a.AttendanceTypeId))
                .Distinct()
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return typedAttendees.ToImmutableList();
        }

        public async Task<int> UpdateKids(
            IImmutableList<PeopleUpdate> kids,
            IImmutableList<BackgroundTasks.Adult.Family> families
        )
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var existingKids = await CommonRepository.GetKidsByPeopleIds(db: db,
                peopleIds: kids.Select(selector: p => p.PeopleId!.Value)
                    .ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);

            return await UpdateKids(db: db, people: existingKids, updates: kids, families: families)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> InsertFamilies(
            IImmutableList<long> newHouseholdIds,
            IImmutableList<PeopleUpdate> peoples
        )
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var families = newHouseholdIds.Select(selector: h => MapFamily(householdId: h, peoples: peoples));
            await db.AddRangeAsync(entities: families).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
            return await GetExistingFamilies(householdIds: newHouseholdIds).ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> GetExistingFamilies(
            IImmutableList<long> householdIds
        )
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var families = await db.Families.Where(predicate: f => householdIds.Contains(f.HouseholdId))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return families.Select(selector: MapFamily).ToImmutableList();
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
                p.Kid.UpdateDate = updateDate;

                p.FirstName = update.FirstName;
                p.LastName = update.LastName;
                p.FamilyId = families.SingleOrDefault(predicate: f => f.HouseholdId == update.HouseholdId)?.FamilyId;
                p.UpdateDate = updateDate;
            });
            
            return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}