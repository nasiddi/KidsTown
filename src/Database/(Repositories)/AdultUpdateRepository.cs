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
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var families = await db.Families
                .Include(navigationPropertyPath: f => f.People)
                .ThenInclude(navigationPropertyPath: p => p.Adult)
                .Include(navigationPropertyPath: f => f.People)
                .ThenInclude(navigationPropertyPath: p => p.Attendances)
                .ToListAsync();

            var filteredFamilies = families.Where(predicate: f => AttendanceIsWithinLookBackWindow(f: f, daysLookBack: daysLookBack))
                .OrderBy(keySelector: f => f.UpdateDate)
                .ThenBy(keySelector: f => f.Id)
                .Take(count: take)
                .ToImmutableList();

            return filteredFamilies.Select(selector: MapFamily).ToImmutableList();
        }
        
        public async Task<int> UpdateAdults(IImmutableList<AdultUpdate> parentUpdates)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

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

            await SetFamilyUpdateDate(db: db, updates: parentUpdates).ConfigureAwait(continueOnCapturedContext: false);
                
            await db.AddRangeAsync(entities: newParents);
            return await db.SaveChangesAsync();
        }

        public async Task<int> RemovePeopleFromFamilies(ImmutableList<long> peopleIds)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var people = await db.People.Where(predicate: p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            
            people.ForEach(action: p => p.FamilyId = null);
            return await db.SaveChangesAsync();
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
                .Where(predicate: f => updates.Select(e => e.FamilyId).Contains(f.Id))
                .ToListAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
            
            var updateDate = DateTime.UtcNow;
            families.ForEach(action: f => f.UpdateDate = updateDate);
        }
        
        private static BackgroundTasks.Adult.Family MapFamily(Family family)
        {
            var members = family.People.Where(predicate: p => p.PeopleId != null)
                .Select(selector: p => new BackgroundTasks.Adult.Person(peopleId: p.PeopleId!.Value)).ToImmutableList();
            
            return new BackgroundTasks.Adult.Family(
                familyId: family.Id,
                householdId: family.HouseholdId,
                members: members);
        }

        private static bool AttendanceIsWithinLookBackWindow(Family f, int daysLookBack)
        {
            return f.People.Any(
                predicate: p => p.Attendances.Any(
                    predicate: a => a.InsertDate > DateTime.Today.AddDays(value: -daysLookBack)));
        }
    }
}