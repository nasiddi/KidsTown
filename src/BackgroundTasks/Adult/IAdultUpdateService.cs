using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.Adult;

public interface IAdultUpdateService
{
    public Task<int> UpdateParents(int daysLookBack, int batchSize);
    Task<int> UpdateVolunteersWithoutFamilies(int daysLookBack, int batchSize);
}