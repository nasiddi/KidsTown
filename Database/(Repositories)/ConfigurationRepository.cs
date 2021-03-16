using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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
        
        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Location>> GetActiveLocations()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.LocationGroups.Where(predicate: l => l.IsEnabled).ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
                return locations.Select(selector: l => new CheckInsExtension.CheckInUpdateJobs.Models.Location(id: l.Id, name: l.Name))
                    .ToImmutableList();
            }
        }
    }
}