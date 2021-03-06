using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using Microsoft.Extensions.Configuration;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfigurationRepository configurationRepository, IConfiguration configuration)
        {
            _configurationRepository = configurationRepository;
            _configuration = configuration;
        }

        public async Task<ImmutableList<Location>> GetActiveLocations()
        {
            return await _configurationRepository.GetActiveLocations();
        }
        
        public IImmutableList<long> GetEventIds()
        {
            return _configuration.GetSection("EventIds").Get<long[]>().ToImmutableList();
        }
    }
}