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
            return await _configurationRepository.GetActiveLocations().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public long GetDefaultEventId()
        {
            return _configuration.GetValue<long>(key: "EventId");
        }

        public async Task<ImmutableList<CheckInsEvent>> GetAvailableEvents()
        {
            var activeEvents = await _planningCenterClient.GetActiveEvents().ConfigureAwait(continueOnCapturedContext: false);
            return activeEvents?.Data?.Select(selector: MapCheckInsEvent).ToImmutableList() ?? ImmutableList<CheckInsEvent>.Empty;
        }

        private static CheckInsEvent MapCheckInsEvent(Datum data)
        {
            return new()
            {
                EventId = data.Id,
                Name = data.Attributes?.Name ?? string.Empty
            };
        }
    }
}