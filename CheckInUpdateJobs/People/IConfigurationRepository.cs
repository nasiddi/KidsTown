using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface IConfigurationRepository
    {
        Task<ImmutableList<Location>> GetActiveLocations();
    }
}