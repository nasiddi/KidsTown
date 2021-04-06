using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IConfigurationService
    {
        Task<ImmutableList<LocationGroup>> GetActiveLocationGroups();
        long GetDefaultEventId();
        Task<ImmutableList<CheckInsEvent>> GetAvailableEvents();
        Task<ImmutableList<Location>> GetLocations(long eventId, IImmutableList<int> selectedLocationGroups);
    }
}