using System.Threading.Tasks;
using KidsTown.BackgroundTasks.PlanningCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.CheckOut
{
    public class AutoCheckOutTask : BackgroundTask
    {
        private readonly IUpdateRepository _updateRepository;

        public AutoCheckOutTask(IUpdateRepository updateRepository, ILoggerFactory loggerFactory, IConfiguration configuration) : base(updateRepository: updateRepository, loggerFactory: loggerFactory, configuration: configuration)
        {
            _updateRepository = updateRepository;
        }

        protected override string TaskName { get; } = nameof(AutoCheckOutTask);
        protected override int Interval { get; } = 3600000;
        protected override Task<int> ExecuteRun()
        {
            return _updateRepository.AutoCheckoutEveryoneByEndOfDay();
        }
    }
}