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
            await using var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>();
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
        
        private static BackgroundTasks.Adult.Family MapFamily(Family family)
        {
            var adults = family.People.Where(predicate: p => p.Adult != null).Select(selector: MapAdult).ToImmutableList();
            
            return new BackgroundTasks.Adult.Family(
                familyId: family.Id,
                householdId: family.HouseholdId,
                adults: adults);
        }

        private static BackgroundTasks.Adult.Adult MapAdult(Person person)
        {
            return new(
                peopleId: person.PeopleId!.Value,
                personId: person.Id,
                firstName: person.FirstName,
                lastName: person.LastName,
                phoneNumber: person.Adult.PhoneNumber);
        }

        private static bool AttendanceIsWithinLookBackWindow(Family f, int daysLookBack)
        {
            return f.People.Any(
                predicate: p => p.Attendances.Any(
                    predicate: a => a.InsertDate > DateTime.Today.AddDays(value: -daysLookBack)));
        }
    }
}