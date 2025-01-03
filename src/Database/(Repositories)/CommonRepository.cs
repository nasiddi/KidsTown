using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Database.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database;

public static class CommonRepository
{
    public static async Task<List<Person>> GetKidsByPeopleIds(
        KidsTownContext db,
        IImmutableList<long> peopleIds
    )
    {
        return await db.People
                .Where(p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .Include(p => p.Kid)
                .ToListAsync();
    }

    public static KidsTownContext GetDatabase(IServiceScopeFactory serviceScopeFactory)
    {
        return serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>();
    }
}