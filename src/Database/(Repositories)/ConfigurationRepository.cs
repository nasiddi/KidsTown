using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database;

public class ConfigurationRepository(IServiceScopeFactory serviceScopeFactory) : IConfigurationRepository
{
    public async Task<IImmutableList<LocationGroup>> GetActiveLocationGroups()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var locations = await db.LocationGroups
                .Where(l => l.IsEnabled)
                .ToListAsync();

        return locations.Select(l => new LocationGroup(l.Id, l.Name))
            .ToImmutableList();
    }

    public async Task<IImmutableList<Location>> GetLocations(long eventId)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var locations = await db.Locations
                .Where(l => l.EventId == eventId)
                .ToListAsync();

        return locations.Select(MapLocation)
            .ToImmutableList();
    }

    private static Location MapLocation(EfCore.Location location)
    {
        return new Location(
            location.Id,
            location.Name,
            location.LocationGroupId);
    }
}