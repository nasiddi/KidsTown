using System.Threading.Tasks;
using KidsTown.BackgroundTasks.PlanningCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Adult
{
    public class AdultUpdateTask : BackgroundTask
    {
        private readonly IAdultUpdateService _adultUpdateService;

        public AdultUpdateTask(
            IAdultUpdateService adultUpdateService,
            IUpdateRepository updateRepository,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(updateRepository: updateRepository,
            loggerFactory: loggerFactory,
            configuration: configuration)
        {
            _adultUpdateService = adultUpdateService;
        }

        protected override string TaskName { get; } = nameof(AdultUpdateTask);
        protected override int Interval { get; } = 5000;
        protected override Task<int> ExecuteRun()
        {
            return _adultUpdateService.UpdateParents(daysLookBack: DaysLookBack, batchSize: 5);
        }
    }
}