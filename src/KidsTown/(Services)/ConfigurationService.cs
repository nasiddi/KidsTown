using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using Microsoft.Extensions.Configuration;

namespace KidsTown.KidsTown
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

        public async Task<IImmutableList<LocationGroup>> GetActiveLocationGroups()
        {
            return await _configurationRepository.GetActiveLocationGroups().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        public long GetDefaultEventId()
        {
            return _configuration.GetValue<long>(key: "EventId");
        }

        public async Task<IImmutableList<CheckInsEvent>> GetAvailableEvents()
        {
            var activeEvents = await _planningCenterClient.GetActiveEvents().ConfigureAwait(continueOnCapturedContext: false);
            return activeEvents?.Data?.Select(selector: MapCheckInsEvent).ToImmutableList() ?? ImmutableList<CheckInsEvent>.Empty;
        }

        public async Task<IImmutableList<Location>> GetLocations(
            long eventId,
            IImmutableList<int>? selectedLocationGroups
        )
        {
            return await _configurationRepository.GetLocations(eventId: eventId, selectedLocationGroups: selectedLocationGroups);
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