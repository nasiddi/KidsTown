using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IConfigurationRepository
    {
        Task<ImmutableList<LocationGroup>> GetActiveLocationGroups();
        Task<ImmutableList<Location>> GetLocations(long eventId, IImmutableList<int> selectedLocationGroups);
    }
}