using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IConfigurationRepository
    {
        Task<ImmutableList<LocationGroup>> GetActiveLocationGroups();
        Task<ImmutableList<Location>> GetLocations(long eventId, IImmutableList<int> selectedLocationGroups);
    }
}