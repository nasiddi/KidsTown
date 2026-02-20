using BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundTasks.Attendance;

public class AttendanceUpdateTask(
        IAttendanceUpdateService attendanceUpdateService,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    : BackgroundTask(backgroundTaskRepository, loggerFactory, configuration)
{
    protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.AttendanceUpdateTask;

    protected override int Interval => 5000;

    protected override int LogFrequency => 36;

    protected override async Task<int> ExecuteRun()
    {
        return await attendanceUpdateService.UpdateAttendance(DaysLookBack);
    }
}