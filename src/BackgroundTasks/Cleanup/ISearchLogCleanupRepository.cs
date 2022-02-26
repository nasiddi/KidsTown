using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.Cleanup
{
    public interface ISearchLogCleanupRepository
    {
        Task<int> ClearOldLogs();
    }
}