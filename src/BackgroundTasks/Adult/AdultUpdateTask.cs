using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Adult
{
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

        protected override string TaskName { get; } = nameof(AdultUpdateTask);
        protected override int Interval { get; } = 45000;
        protected override Task<int> ExecuteRun()
        {
            return _adultUpdateService.UpdateParents(daysLookBack: DaysLookBack, batchSize: 20);
        }
    }
}