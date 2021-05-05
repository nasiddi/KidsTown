using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.CheckOut
{
    public class AutoCheckOutTask : BackgroundTask
    {
        private readonly IAttendanceUpdateRepository _attendanceUpdateRepository;

        public AutoCheckOutTask(
            IAttendanceUpdateRepository attendanceUpdateRepository,
            IBackgroundTaskRepository backgroundTaskRepository,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(backgroundTaskRepository: backgroundTaskRepository,
            loggerFactory: loggerFactory,
            configuration: configuration)
        {
            _attendanceUpdateRepository = attendanceUpdateRepository;
        }

        protected override string TaskName { get; } = nameof(AutoCheckOutTask);
        protected override int Interval { get; } = 2700000;
        protected override Task<int> ExecuteRun()
        {
            return _attendanceUpdateRepository.AutoCheckoutEveryoneByEndOfDay();
        }
    }
}