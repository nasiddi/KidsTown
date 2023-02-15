using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Adult;

public class AdultUpdateTask : BackgroundTask
{
    private readonly IAdultUpdateService _adultUpdateService;

    public AdultUpdateTask(
        IAdultUpdateService adultUpdateService,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration
    ) : base(backgroundTaskRepository: backgroundTaskRepository,
        loggerFactory: loggerFactory,
        configuration: configuration)
    {
        _adultUpdateService = adultUpdateService;
    }

    protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.AdultUpdateTask;
    protected override int Interval => 15000;
    protected override int LogFrequency => 4;

    protected override async Task<int> ExecuteRun()
    {
        var updateCount = await _adultUpdateService.UpdateParents(daysLookBack: DaysLookBack, batchSize: 50);
        updateCount += await _adultUpdateService.UpdateVolunteersWithoutFamilies(daysLookBack: DaysLookBack, batchSize: 50);
        return updateCount;
    }
}