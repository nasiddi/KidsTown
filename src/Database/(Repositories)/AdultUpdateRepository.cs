using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.Database.EfCore;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Family = KidsTown.BackgroundTasks.Adult.Family;
using Person = KidsTown.Database.EfCore.Person;

namespace KidsTown.Database;

public class AdultUpdateRepository(IServiceScopeFactory serviceScopeFactory) : IAdultUpdateRepository
{
    public async Task<IImmutableList<Family>> GetFamiliesToUpdate(int daysLookBack, int take)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var personIds = await db.Attendances.Where(
                a
                    => a.InsertDate >= DateTime.Today.AddDays(-daysLookBack)
                    && a.AttendanceTypeId == (int) AttendanceTypeId.Regular)
            .Select(a => a.PersonId)
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        var familyIds = await db.People
            .Where(p => p.FamilyId.HasValue && personIds.Contains(p.Id))
            .Select(p => p.FamilyId)
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        var families = await db.Families
            .Include(f => f.People)
            .Where(
                f => familyIds.Contains(f.Id)
                    && f.HouseholdId.HasValue)
            .OrderBy(f => f.UpdateDate)
            .Take(take)
            .ToListAsync();

        return families.Select(MapFamily).ToImmutableList();
    }

    public async Task<ImmutableList<long>> GetVolunteerPersonIdsWithoutFamiliesToUpdate(int daysLookBack, int take)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var personIds = await db.Attendances.Where(
                a
                    => a.InsertDate >= DateTime.Today.AddDays(-daysLookBack)
                    && a.AttendanceTypeId == (int) AttendanceTypeId.Volunteer)
            .Select(a => a.PersonId)
            .Distinct()
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        return (await db.People
                .Where(p => !p.FamilyId.HasValue && personIds.Contains(p.Id) && p.PeopleId != null)
                .OrderBy(f => f.UpdateDate)
                .Select(p => p.PeopleId!.Value)
                .Distinct()
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(continueOnCapturedContext: false))
            .ToImmutableList();
    }

    public async Task<int> UpdateVolunteers(
        ImmutableList<long> peopleIds,
        ImmutableList<VolunteerUpdate> volunteerUpdates)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);
        var people = await db.People
            .Where(e => e.PeopleId.HasValue && peopleIds.Contains(e.PeopleId!.Value))
            .ToListAsync();

        foreach (var volunteerUpdate in volunteerUpdates)
        {
            var person = people.Single(e => e.PeopleId == volunteerUpdate.PeopleId);
            person.FirstName = volunteerUpdate.FirstName;
            person.LastName = volunteerUpdate.LastName;
        }

        var updateDate = DateTime.UtcNow;

        foreach (var person in people)
        {
            person.UpdateDate = updateDate;
        }

        return await db.SaveChangesAsync();
    }

    public async Task<int> UpdateAdults(IImmutableList<AdultUpdate> parentUpdates)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var peopleIds = parentUpdates.Select(p => p.PeopleId).ToImmutableList();
        var existingParents = await GetExistingParents(peopleIds, db).ConfigureAwait(continueOnCapturedContext: false);

        var updates = parentUpdates.Where(p => existingParents.Select(e => e.PeopleId).Contains(p.PeopleId))
            .ToImmutableList();

        foreach (var existingParent in existingParents)
        {
            UpdateParent(existingParent, updates);
        }

        var newEntries = parentUpdates.Except(updates).ToImmutableList();
        var newParents = newEntries.Select(MapParent);

        await db.AddRangeAsync(newParents);
        return await db.SaveChangesAsync();
    }

    public async Task<int> RemovePeopleFromFamilies(ImmutableList<long> peopleIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var people = await db.People.Where(p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        people.ForEach(p => p.FamilyId = null);
        return await db.SaveChangesAsync();
    }

    public async Task<int> SetFamilyUpdateDate(IImmutableList<Family> families)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var persistedFamilies = await db.Families
            .Where(f => families.Select(e => e.FamilyId).Contains(f.Id))
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        var updateDate = DateTime.UtcNow;
        persistedFamilies.ForEach(f => f.UpdateDate = updateDate);

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
            .ConfigureAwait(continueOnCapturedContext: false);

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

        person.Adult ??= new Adult
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

    private static Family MapFamily(EfCore.Family family)
    {
        var members = family.People.Where(p => p.PeopleId != null)
            .Select(p => new BackgroundTasks.Adult.Person(p.PeopleId!.Value, null))
            .ToImmutableList();

        return new Family(
            family.Id,
            family.HouseholdId!.Value,
            members);
    }
}