using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Attendance
{
    public class AttendanceUpdateTask : BackgroundTask
    {
        protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.AttendanceUpdateTask;
        protected override int Interval => 5000;
        protected override int LogFrequency => 36;

        private readonly IAttendanceUpdateService _attendanceUpdateService;

        public AttendanceUpdateTask(
            IAttendanceUpdateService attendanceUpdateService,
            IBackgroundTaskRepository backgroundTaskRepository,
            ILoggerFactory loggerFactory,
            IConfiguration configuration
        ) : base(backgroundTaskRepository: backgroundTaskRepository, loggerFactory: loggerFactory, configuration: configuration)
        {
            _attendanceUpdateService = attendanceUpdateService;
        }

        protected override async Task<int> ExecuteRun()
        {
            return await _attendanceUpdateService.UpdateAttendance(daysLookBack: DaysLookBack)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}