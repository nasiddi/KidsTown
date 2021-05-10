using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database
{
    public class PeopleRepository : IPeopleRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public PeopleRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IImmutableList<KidsTown.Models.Adult>> GetParents(IImmutableList<int> attendanceIds)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var familyIds = await db.Attendances
                .Include(a => a.Person)
                .Where(a => attendanceIds.Contains(a.Id) && a.Person.FamilyId != null)
                .Select(a => a.Person.FamilyId!.Value)
                .Distinct()
                .ToListAsync();

            return await GetAdults(familyIds.ToImmutableList());
        }
        
        public async Task<IImmutableList<KidsTown.Models.Adult>> GetAdults(IImmutableList<int> familyIds)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            
            var adults = await (from a in db.Adults
                    join p in db.People
                        on a.PersonId equals p.Id
                    where p.FamilyId.HasValue && familyIds.Contains(p.FamilyId.Value)
                    select MapAdult(p, a))
                .ToListAsync().ConfigureAwait(false);
                
            return adults?.ToImmutableList()
                   ?? ImmutableList<KidsTown.Models.Adult>.Empty;
        }

        private static KidsTown.Models.Adult MapAdult(Person person, Adult adult)
        {
            return new()
            {
                FamilyId = person.FamilyId!.Value,
                FirstName = person.FirstName,
                LastName = person.LastName,
                PhoneNumber = adult.PhoneNumber
            };
        }
    }
}