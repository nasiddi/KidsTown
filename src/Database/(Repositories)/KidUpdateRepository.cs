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
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            
            var typedAttendees = await db.Attendances
                .Include(a => a.Person)
                .Where(i
                    => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Person.PeopleId.HasValue)
                .OrderBy(a => a.Person.UpdateDate)
                .ThenBy(a => a.Person.Id)
                .Take(take)
                .Select(a => new TypedAttendee(a.Person.PeopleId!.Value, (AttendanceTypeId)a.AttendanceTypeId))
                .Distinct()
                .ToListAsync().ConfigureAwait(false);

            return typedAttendees.ToImmutableList();
        }

        public async Task<int> UpdateKids(
            IImmutableList<PeopleUpdate> kids,
            IImmutableList<BackgroundTasks.Adult.Family> families
        )
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var existingKids = await CommonRepository.GetKidsByPeopleIds(db: db,
                peopleIds: kids.Select(p => p.PeopleId!.Value)
                    .ToImmutableList()).ConfigureAwait(false);

            return await UpdateKids(db: db, people: existingKids, updates: kids, families: families)
                .ConfigureAwait(false);
        }
        
        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> InsertFamilies(
            IImmutableList<long> newHouseholdIds,
            IImmutableList<PeopleUpdate> peoples
        )
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var families = newHouseholdIds.Select(h => MapFamily(householdId: h, peoples: peoples));
            await db.AddRangeAsync(families).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
            return await GetExistingFamilies(newHouseholdIds).ConfigureAwait(false);
        }
        
        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> GetExistingFamilies(
            IImmutableList<long> householdIds
        )
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var families = await db.Families.Where(f => householdIds.Contains(f.HouseholdId))
                .ToListAsync().ConfigureAwait(false);

            return families.Select(MapFamily).ToImmutableList();
        }

        private static Family MapFamily(long householdId, IImmutableList<PeopleUpdate> peoples)
        {
            var name = peoples.First(p => p.HouseholdId == householdId).HouseholdName;

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
            var updatesByPeopleId = updates.Where(u => u.PeopleId.HasValue)
                .ToImmutableDictionary(keySelector: k => k.PeopleId!.Value, elementSelector: v => v);

            var updateDate = DateTime.UtcNow;
            
            people.ForEach(p =>
            {
                if (p.PeopleId == null)
                {
                    return;
                }
               
                var update = updatesByPeopleId[p.PeopleId.Value];

                p.Kid ??= new Kid();

                p.Kid.MayLeaveAlone = update.MayLeaveAlone;
                p.Kid.HasPeopleWithoutPickupPermission = update.HasPeopleWithoutPickupPermission;
                p.Kid.UpdateDate = updateDate;

                p.FirstName = update.FirstName;
                p.LastName = update.LastName;
                p.FamilyId = families.SingleOrDefault(f => f.HouseholdId == update.HouseholdId)?.FamilyId;
                
                p.UpdateDate = updateDate;
            });
            
            return await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}