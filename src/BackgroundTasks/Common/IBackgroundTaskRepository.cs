namespace BackgroundTasks.Common;

public interface IBackgroundTaskRepository
{
    Task LogTaskRun(bool success, int updateCount, string environment, string taskName);
}