using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown;

public interface IConfigurationRepository
{
    Task<IImmutableList<LocationGroup>> GetActiveLocationGroups();
    Task<IImmutableList<Location>> GetLocations(long eventId, IImmutableList<int>? selectedLocationGroups);
}