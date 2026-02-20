namespace BackgroundTasks.Cleanup;

public interface ISearchLogCleanupRepository
{
    Task<int> ClearOldLogs();
}