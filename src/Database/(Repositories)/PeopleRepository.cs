using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Database.EfCore;
using KidsTown.KidsTown;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database;

public class PeopleRepository : IPeopleRepository
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public PeopleRepository(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<IImmutableList<KidsTown.Models.Adult>> GetParents(IImmutableList<int> attendanceIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

        var familyIds = await db.Attendances
            .Include(navigationPropertyPath: a => a.Person)
            .Where(predicate: a => attendanceIds.Contains(a.Id) && a.Person.FamilyId != null)
            .Select(selector: a => a.Person.FamilyId!.Value)
            .Distinct()
            .ToListAsync();

        return await GetAdults(familyIds: familyIds.ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);
    }
        
    public async Task<IImmutableList<KidsTown.Models.Adult>> GetAdults(IImmutableList<int> familyIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
        var adults = await (from a in db.Adults
                join p in db.People
                    on a.PersonId equals p.Id
                where p.FamilyId.HasValue && familyIds.Contains(p.FamilyId.Value)
                select MapAdult(p, a))
            .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                
        return adults.OrderByDescending(keySelector: a => a.IsPrimaryContact).ToImmutableList();
    }
    public async Task UpdateAdults(IImmutableList<KidsTown.Models.Adult> adults)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
        var persistedAdults = await db.Adults
            .Include(navigationPropertyPath: a => a.Person)
            .Where(predicate: a => adults.Select(ad => ad.PersonId).Contains(a.PersonId))
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);
            
        persistedAdults.ForEach(action: a =>
        {
            var update = adults.Single(predicate: u => u.PersonId == a.PersonId);

            a.IsPrimaryContact = update.IsPrimaryContact;
            a.PhoneNumber = update.PhoneNumber;
        });

        await db.SaveChangesAsync();
    }

    public async Task InsertUnregisteredGuest(string securityCode, int locationId)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

        var attendanceCount = await db.Attendances.
            CountAsync(predicate: a => a.SecurityCode == securityCode && a.InsertDate >= DateTime.Today);

        if (attendanceCount > 0)
        {
            return;
        }

        var attendance = new Attendance
        {
            PersonId = 1,
            LocationId = locationId,
            SecurityCode = securityCode,
            AttendanceTypeId = (int) AttendanceTypeId.Guest,
            InsertDate = DateTime.UtcNow
        };

        db.Add(entity: attendance);
        await db.SaveChangesAsync();
    }

    private static KidsTown.Models.Adult MapAdult(Person person, Adult adult)
    {
        return new()
        {
            PeopleId = person.PeopleId,
            FamilyId = person.FamilyId!.Value,
            PersonId = person.Id,
            PhoneNumberId = adult.PhoneNumberId,
            FirstName = person.FirstName,
            LastName = person.LastName,
            PhoneNumber = adult.PhoneNumber ?? string.Empty,
            IsPrimaryContact = adult.IsPrimaryContact
        };
    }
}