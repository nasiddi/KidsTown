using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown;

public interface IConfigurationService
{
    Task<IImmutableList<LocationGroup>> GetActiveLocationGroups();
    long GetDefaultEventId();
    Task<IImmutableList<CheckInsEvent>> GetAvailableEvents();
    Task<IImmutableList<Location>> GetLocations(long eventId);
}