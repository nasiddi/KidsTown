using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IConfigurationRepository
{
    Task<IImmutableList<LocationGroup>> GetActiveLocationGroups();
    Task<IImmutableList<Location>> GetLocations(long eventId);
}