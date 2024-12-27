using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.Kid;
using KidsTown.Database.EfCore;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Family = KidsTown.BackgroundTasks.Adult.Family;
using Person = KidsTown.Database.EfCore.Person;

namespace KidsTown.Database;

public class KidUpdateRepository(IServiceScopeFactory serviceScopeFactory) : IKidUpdateRepository
{
    public async Task<ImmutableList<long>> GetKidsPeopleIdToUpdate(int daysLookBack, int take)
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

        var attendees = await db.People
            .Include(p => p.Kid)
            .Where(
                p => p.PeopleId.HasValue
                    && personIds.Contains(p.Id))
            .OrderBy(p => p.Kid!.UpdateDate)
            .Take(take)
            .Select(p => p.PeopleId!.Value)
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        return attendees.ToImmutableList();
    }

    public async Task<int> UpdateKids(
        IImmutableList<PeopleUpdate> kids,
        IImmutableList<Family> families
    )
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var existingKids = await CommonRepository.GetKidsByPeopleIds(
                db,
                kids.Select(p => p.PeopleId!.Value)
                    .ToImmutableList())
            .ConfigureAwait(continueOnCapturedContext: false);

        return await UpdateKids(db, existingKids, kids, families)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<IImmutableList<Family>> InsertFamilies(
        IImmutableList<long> newHouseholdIds,
        IImmutableList<PeopleUpdate> peoples
    )
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var families = newHouseholdIds.Select(h => MapFamily(h, peoples));
        await db.AddRangeAsync(families).ConfigureAwait(continueOnCapturedContext: false);
        await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        return await GetExistingFamilies(newHouseholdIds).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<IImmutableList<Family>> GetExistingFamilies(
        IImmutableList<long> householdIds
    )
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var families = await db.Families.Where(
                f => f.HouseholdId.HasValue
                    && householdIds.Contains(f.HouseholdId.Value))
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        return families.Select(MapFamily).ToImmutableList();
    }

    private static EfCore.Family MapFamily(long householdId, IImmutableList<PeopleUpdate> peoples)
    {
        var name = peoples.First(p => p.HouseholdId == householdId).HouseholdName;

        return new EfCore.Family
        {
            HouseholdId = householdId,
            Name = name!,
            UpdateDate = DateTime.UnixEpoch
        };
    }

    private static Family MapFamily(EfCore.Family family)
    {
        return new Family(
            family.Id,
            family.HouseholdId!.Value,
            ImmutableList<BackgroundTasks.Adult.Person>.Empty);
    }

    private static async Task<int> UpdateKids(
        DbContext db,
        List<Person> people,
        IImmutableList<PeopleUpdate> updates,
        IImmutableList<Family> families
    )
    {
        var updatesByPeopleId = updates.Where(u => u.PeopleId.HasValue)
            .ToImmutableDictionary(k => k.PeopleId!.Value, v => v);

        var updateDate = DateTime.UtcNow;

        people.ForEach(
            p =>
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

        return await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
    }
}