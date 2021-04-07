using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToUsingDeclaration

namespace KidsTown.Database
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ConfigurationRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<ImmutableList<KidsTown.Models.LocationGroup>> GetActiveLocationGroups()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.LocationGroups
                    .Where(predicate: l => l.IsEnabled)
                    .ToListAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
                
                return locations.Select(selector: l => new KidsTown.Models.LocationGroup(id: l.Id, name: l.Name))
                    .ToImmutableList();
            }
        }

        public async Task<ImmutableList<KidsTown.Models.Location>> GetLocations(
            long eventId,
            IImmutableList<int>? selectedLocationGroups
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.Locations
                    .Where(predicate: l => l.EventId == eventId 
                        && (selectedLocationGroups == null || selectedLocationGroups.Contains(l.LocationGroupId)))
                    .ToListAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
                
                return locations.Select(selector: MapLocation)
                    .ToImmutableList();
            }
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