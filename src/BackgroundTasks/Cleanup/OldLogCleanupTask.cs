using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Cleanup
{
    public class OldLogCleanupTask : BackgroundTask
    {
        private readonly ISearchLogCleanupRepository _searchLogCleanupRepository;

        public OldLogCleanupTask(
            IBackgroundTaskRepository backgroundTaskRepository,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ISearchLogCleanupRepository searchLogCleanupRepository
        )
            : base(backgroundTaskRepository, loggerFactory, configuration)
        {
            _searchLogCleanupRepository = searchLogCleanupRepository;
        }

        protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.OldLogCleanupTask;
        protected override int Interval => 2700000;
        protected override int LogFrequency => 1;
        protected override async Task<int> ExecuteRun()
        {
            return await _searchLogCleanupRepository.ClearOldLogs();
        }
    }
}