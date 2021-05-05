using System.Collections.Immutable;
using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.Adult
{
    public interface IAdultUpdateRepository
    {
        Task<IImmutableList<Family>> GetFamiliesToUpdate(int daysLookBack, int take);
        Task<int> UpdateAdults(IImmutableList<AdultUpdate> parentUpdates);
        Task<int> RemovePeopleFromFamilies(ImmutableList<long> peopleIds);
    }
}
