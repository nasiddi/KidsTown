using System.Collections.Immutable;
using KidsTown.Models;
using Microsoft.Extensions.Configuration;
using PlanningCenterApiClient;
using PlanningCenterApiClient.Models.EventResult;

namespace KidsTown;

public class ConfigurationService(
        IConfigurationRepository configurationRepository,
        IConfiguration configuration,
        IPlanningCenterClient planningCenterClient)
    : IConfigurationService
{
    public async Task<IImmutableList<LocationGroup>> GetActiveLocationGroups()
    {
        return await configurationRepository.GetActiveLocationGroups();
    }

    public long GetDefaultEventId()
    {
        return configuration.GetValue<long>("EventId");
    }

    public async Task<IImmutableList<CheckInsEvent>> GetAvailableEvents()
    {
        var activeEvents = await planningCenterClient.GetActiveEvents();
        return activeEvents?.Data?.Select(MapCheckInsEvent).ToImmutableList() ?? ImmutableList<CheckInsEvent>.Empty;
    }

    public async Task<IImmutableList<Location>> GetLocations(long eventId)
    {
        return await configurationRepository.GetLocations(eventId);
    }

    private static CheckInsEvent MapCheckInsEvent(Datum data)
    {
        return new CheckInsEvent
        {
            EventId = data.Id,
            Name = data.Attributes?.Name ?? string.Empty
        };
    }
}