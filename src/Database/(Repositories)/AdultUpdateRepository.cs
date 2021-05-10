using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database
{
    public class AdultUpdateRepository : IAdultUpdateRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AdultUpdateRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<IImmutableList<BackgroundTasks.Adult.Family>> GetFamiliesToUpdate(int daysLookBack, int take)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            
            var families = await db.Families
                .Include(f => f.People)
                .ThenInclude(p => p.Adult)
                .Include(f => f.People)
                .ThenInclude(p => p.Attendances)
                .ToListAsync();

            var filteredFamilies = families.Where(f => AttendanceIsWithinLookBackWindow(f: f, daysLookBack: daysLookBack))
                .OrderBy(f => f.UpdateDate)
                .ThenBy(f => f.Id)
                .Take(take)
                .ToImmutableList();

            return filteredFamilies.Select(MapFamily).ToImmutableList();
        }
        
        public async Task<int> UpdateAdults(IImmutableList<AdultUpdate> parentUpdates)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var peopleIds = parentUpdates.Select(p => p.PeopleId).ToImmutableList();
            var existingParents = await GetExistingParents(peopleIds: peopleIds, db: db).ConfigureAwait(false);

            var updates = parentUpdates.Where(
                    p => existingParents.Select(e => e.PeopleId).Contains(p.PeopleId))
                .ToImmutableList();
                
            foreach (var existingParent in existingParents)
            {
                UpdateParent(person: existingParent, updates: updates);
            }
                
            var newEntries = parentUpdates.Except(updates).ToImmutableList();
            var newParents = newEntries.Select(MapParent);

            await SetFamilyUpdateDate(db: db, updates: parentUpdates).ConfigureAwait(false);
                
            await db.AddRangeAsync(newParents);
            return await db.SaveChangesAsync();
        }

        public async Task<int> RemovePeopleFromFamilies(ImmutableList<long> peopleIds)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var people = await db.People.Where(p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .ToListAsync().ConfigureAwait(false);
            
            people.ForEach(p => p.FamilyId = null);
            return await db.SaveChangesAsync();
        }

        private static async Task<IImmutableList<Person>> GetExistingParents(
            IImmutableList<long> peopleIds,
            KidsTownContext db
        )
        {
            var parents = await db.People
                .Where(p => p.PeopleId != null && peopleIds.Contains(p.PeopleId.Value))
                .Include(p => p.Adult)
                .Include(p => p.Family)
                .ToListAsync()
                .ConfigureAwait(false);
            return parents.ToImmutableList();
        }
        
        private static void UpdateParent(Person person, IImmutableList<AdultUpdate> updates)
        {
            var update = updates.Single(u => u.PeopleId == person.PeopleId);

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
        
        private static async Task SetFamilyUpdateDate(KidsTownContext db, IImmutableList<AdultUpdate> updates)
        {
            var families = await db.Families
                .Where(f => updates.Select(e => e.FamilyId).Contains(f.Id))
                .ToListAsync()
                .ConfigureAwait(false);
            
            var updateDate = DateTime.UtcNow;
            families.ForEach(f => f.UpdateDate = updateDate);
        }
        
        private static BackgroundTasks.Adult.Family MapFamily(Family family)
        {
            var members = family.People.Where(p => p.PeopleId != null)
                .Select(p => new BackgroundTasks.Adult.Person(p.PeopleId!.Value)).ToImmutableList();
            
            return new BackgroundTasks.Adult.Family(
                familyId: family.Id,
                householdId: family.HouseholdId,
                members: members);
        }

        private static bool AttendanceIsWithinLookBackWindow(Family f, int daysLookBack)
        {
            return f.People.Any(
                p => p.Attendances.Any(
                    a => a.InsertDate > DateTime.Today.AddDays(-daysLookBack)));
        }
    }
}