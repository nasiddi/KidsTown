using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IConfigurationService
{
    Task<IImmutableList<LocationGroup>> GetActiveLocationGroups();
    long GetDefaultEventId();
    Task<IImmutableList<CheckInsEvent>> GetAvailableEvents();
    Task<IImmutableList<Location>> GetLocations(long eventId);
}