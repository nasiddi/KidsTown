using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ConfigurationRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<IImmutableList<KidsTown.Models.LocationGroup>> GetActiveLocationGroups()
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            
            var locations = await db.LocationGroups
                .Where(l => l.IsEnabled)
                .ToListAsync()
                .ConfigureAwait(false);
                
            return locations.Select(l => new KidsTown.Models.LocationGroup(id: l.Id, name: l.Name))
                .ToImmutableList();
        }

        public async Task<IImmutableList<KidsTown.Models.Location>> GetLocations(
            long eventId,
            IImmutableList<int>? selectedLocationGroups
        )
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);
            
            var locations = await db.Locations
                .Where(l => l.EventId == eventId 
                                       && (selectedLocationGroups == null || selectedLocationGroups.Contains(l.LocationGroupId)))
                .ToListAsync()
                .ConfigureAwait(false);
                
            return locations.Select(MapLocation)
                .ToImmutableList();
        }

        private static KidsTown.Models.Location MapLocation(Location location)
        {
            return new(
                id: location.Id,
                name: location.Name,
                locationGroupId: location.LocationGroupId);
        }
    }
}