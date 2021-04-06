using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ConvertToUsingDeclaration

namespace ChekInsExtension.Database
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ConfigurationRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.LocationGroup>> GetActiveLocationGroups()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.LocationGroups
                    .Where(predicate: l => l.IsEnabled)
                    .ToListAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
                
                return locations.Select(selector: l => new CheckInsExtension.CheckInUpdateJobs.Models.LocationGroup(id: l.Id, name: l.Name))
                    .ToImmutableList();
            }
        }

        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Location>> GetLocations(
            long eventId,
            IImmutableList<int> selectedLocationGroups
        )
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.Locations
                    .Where(predicate: l => l.EventId == eventId 
                        && selectedLocationGroups.Contains(l.LocationGroupId))
                    .ToListAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
                
                return locations.Select(selector: l => new CheckInsExtension.CheckInUpdateJobs.Models.Location(id: l.Id, name: l.Name, locationGroupId: l.LocationGroupId))
                    .ToImmutableList();
            }
        }
    }
}