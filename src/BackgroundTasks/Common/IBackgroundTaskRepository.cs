using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.Common;

public interface IBackgroundTaskRepository
{
    Task LogTaskRun(bool success, int updateCount, string environment, string taskName);
}