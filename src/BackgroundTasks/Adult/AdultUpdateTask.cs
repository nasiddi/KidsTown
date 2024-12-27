using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Adult;

public class AdultUpdateTask(
        IAdultUpdateService adultUpdateService,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    : BackgroundTask(
        backgroundTaskRepository,
        loggerFactory,
        configuration)
{
    protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.AdultUpdateTask;

    protected override int Interval => 15000;

    protected override int LogFrequency => 4;

    protected override async Task<int> ExecuteRun()
    {
        var updateCount = await adultUpdateService.UpdateParents(DaysLookBack, batchSize: 50);
        updateCount += await adultUpdateService.UpdateVolunteersWithoutFamilies(DaysLookBack, batchSize: 50);
        return updateCount;
    }
}