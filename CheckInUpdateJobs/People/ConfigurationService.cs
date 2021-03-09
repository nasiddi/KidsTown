using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.PlanningCenterAPIClient;
using CheckInsExtension.PlanningCenterAPIClient.Models.EventResult;
using Microsoft.Extensions.Configuration;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IConfiguration _configuration;
        private readonly IPlanningCenterClient _planningCenterClient;

        public ConfigurationService(
            IConfigurationRepository configurationRepository, 
            IConfiguration configuration, 
            IPlanningCenterClient planningCenterClient)
        {
            _configurationRepository = configurationRepository;
            _configuration = configuration;
            _planningCenterClient = planningCenterClient;
        }

        public async Task<ImmutableList<Location>> GetActiveLocations()
        {
            return await _configurationRepository.GetActiveLocations();
        }
        
        public long GetDefaultEventId()
        {
            return _configuration.GetValue<long>("EventId");
        }

        public async Task<ImmutableList<CheckInsEvent>> GetAvailableEvents()
        {
            var activeEvents = await _planningCenterClient.GetActiveEvents();
            return activeEvents.Data.Select(MapCheckInsEvent).ToImmutableList();
        }

        private static CheckInsEvent MapCheckInsEvent(Datum data)
        {
            return new()
            {
                EventId = data.Id,
                Name = data.Attributes.Name,
            };
        }
    }
}