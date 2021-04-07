using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IConfigurationService
    {
        Task<ImmutableList<LocationGroup>> GetActiveLocationGroups();
        long GetDefaultEventId();
        Task<ImmutableList<CheckInsEvent>> GetAvailableEvents();
        Task<ImmutableList<Location>> GetLocations(long eventId, IImmutableList<int>? selectedLocationGroups);
    }
}