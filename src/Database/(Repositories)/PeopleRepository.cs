using System.Collections.Immutable;
using Database.EfCore;
using KidsTown;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Adult = KidsTown.Models.Adult;

namespace Database;

public class PeopleRepository(IServiceScopeFactory serviceScopeFactory) : IPeopleRepository
{
    public async Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var familyIds = await db.Attendances
            .Include(a => a.Person)
            .Where(a => attendanceIds.Contains(a.Id) && a.Person.FamilyId != null)
            .Select(a => a.Person.FamilyId!.Value)
            .Distinct()
            .ToListAsync();

        return await GetAdults(familyIds.ToImmutableList());
    }

    public async Task<IImmutableList<Adult>> GetAdults(IImmutableList<int> familyIds)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var adults = await (from a in db.Adults
                    join p in db.People
                        on a.PersonId equals p.Id
                    where p.FamilyId.HasValue && familyIds.Contains(p.FamilyId.Value)
                    select MapAdult(p, a))
                .ToListAsync();

        return adults.OrderByDescending(a => a.IsPrimaryContact).ToImmutableList();
    }

    public async Task UpdateAdults(IImmutableList<Adult> adults)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);
        var persistedAdults = await db.Adults
                .Include(a => a.Person)
                .Where(a => adults.Select(ad => ad.PersonId).Contains(a.PersonId))
                .ToListAsync();

        persistedAdults.ForEach(
            a =>
            {
                var update = adults.Single(u => u.PersonId == a.PersonId);

                a.IsPrimaryContact = update.IsPrimaryContact;
                a.PhoneNumber = update.PhoneNumber;
            });

        await db.SaveChangesAsync();
    }

    public async Task InsertUnregisteredGuest(string securityCode, int locationId)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var attendanceCount = await db.Attendances.CountAsync(a => a.SecurityCode == securityCode && a.InsertDate >= DateTime.Today);

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

        db.Add(attendance);
        await db.SaveChangesAsync();
    }

    private static Adult MapAdult(Person person, EfCore.Adult adult)
    {
        return new Adult
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