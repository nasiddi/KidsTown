using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.Database.EfCore;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Family = KidsTown.Database.EfCore.Family;
using Person = KidsTown.Database.EfCore.Person;

namespace KidsTown.Database;

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
            
        var personIds = await db.Attendances.Where(predicate: a
                => a.InsertDate >= DateTime.Today.AddDays(-daysLookBack) 
                && a.AttendanceTypeId == (int) AttendanceTypeId.Regular)
            .Select(selector: a => a.PersonId)
            .Distinct()
            .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            
        var familyIds = await db.People
            .Where(predicate: p => p.FamilyId.HasValue && personIds.Contains(p.Id))
            .Select(selector: p => p.FamilyId)
            .Distinct()
            .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                
        var families = await db.Families
            .Include(navigationPropertyPath: f => f.People)
            .Where(predicate: f => familyIds.Contains(f.Id)
                && f.HouseholdId.HasValue)
            .OrderBy(keySelector: f => f.UpdateDate)
            .Take(count: take)
            .ToListAsync();

        return families.Select(selector: MapFamily).ToImmutableList();
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
        
    public async Task<int> SetFamilyUpdateDate(IImmutableList<BackgroundTasks.Adult.Family> families)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
        var persistedFamilies = await db.Families
            .Where(predicate: f => families.Select(e => e.FamilyId).Contains(f.Id))
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);
            
        var updateDate = DateTime.UtcNow;
        persistedFamilies.ForEach(action: f => f.UpdateDate = updateDate);
            
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

        person.Adult ??= new()
        {
            IsPrimaryContact = false
        };

        person.Adult.PhoneNumberId = update.PhoneNumberId;
        person.Adult.PhoneNumber = update.PhoneNumber;
            

        var updateDate = DateTime.UtcNow;
        person.UpdateDate = updateDate;
    }

    private static Person MapParent(AdultUpdate adultUpdate)
    {
        var adult = new Adult
        {
            PhoneNumberId = adultUpdate.PhoneNumberId,
            PhoneNumber = adultUpdate.PhoneNumber,
            IsPrimaryContact = false
        };

        return new()
        {
            PeopleId = adultUpdate.PeopleId,
            FamilyId = adultUpdate.FamilyId,
            FirstName = adultUpdate.FirstName,
            LastName = adultUpdate.LastName,
            UpdateDate = DateTime.UtcNow,
            Adult = adult
        };
    }

    private static BackgroundTasks.Adult.Family MapFamily(Family family)
    {
        var members = family.People.Where(predicate: p => p.PeopleId != null)
            .Select(selector: p => new BackgroundTasks.Adult.Person(peopleId: p.PeopleId!.Value)).ToImmutableList();
            
        return new(
            familyId: family.Id,
            householdId: family.HouseholdId!.Value,
            members: members);
    }
}