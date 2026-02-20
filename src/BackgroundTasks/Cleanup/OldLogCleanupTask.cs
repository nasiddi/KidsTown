using BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundTasks.Cleanup;

public class OldLogCleanupTask(
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        ISearchLogCleanupRepository searchLogCleanupRepository)
    : BackgroundTask(
        backgroundTaskRepository,
        loggerFactory,
        configuration)
{
    protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.OldLogCleanupTask;

    protected override int Interval => 2700000;

    protected override int LogFrequency => 1;

    protected override async Task<int> ExecuteRun()
    {
        return await searchLogCleanupRepository.ClearOldLogs();
    }
}