using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Kid
{
    public class KidUpdateTask: BackgroundTask
    {
        private readonly IKidUpdateService _kidUpdateService;

        public KidUpdateTask(
            IKidUpdateService kidUpdateService,
            IBackgroundTaskRepository backgroundTaskRepository,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(backgroundTaskRepository: backgroundTaskRepository,
            loggerFactory: loggerFactory,
            configuration: configuration)
        {
            _kidUpdateService = kidUpdateService;
        }

        protected override BackgroundTaskType BackgroundTaskType { get; } = BackgroundTaskType.KidUpdateTask;
        protected override int Interval { get; } = 30000;
        protected override int LogFrequency { get; } = 15;

        protected override Task<int> ExecuteRun()
        {
            return _kidUpdateService.UpdateKids(daysLookBack: DaysLookBack, batchSize: 80);
        }
    }
}