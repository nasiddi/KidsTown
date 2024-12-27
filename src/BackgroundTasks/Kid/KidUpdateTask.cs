using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Kid;

public class KidUpdateTask(
        IKidUpdateService kidUpdateService,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    : BackgroundTask(
        backgroundTaskRepository,
        loggerFactory,
        configuration)
{
    protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.KidUpdateTask;

    protected override int Interval => 30000;

    protected override int LogFrequency => 15;

    protected override Task<int> ExecuteRun()
    {
        return kidUpdateService.UpdateKids(DaysLookBack, batchSize: 100);
    }
}