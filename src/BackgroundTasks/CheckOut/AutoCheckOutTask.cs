using BackgroundTasks.Attendance;
using BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundTasks.CheckOut;

public class AutoCheckOutTask(
        IAttendanceUpdateRepository attendanceUpdateRepository,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    : BackgroundTask(
        backgroundTaskRepository,
        loggerFactory,
        configuration)
{
    protected override BackgroundTaskType BackgroundTaskType => BackgroundTaskType.AutoCheckOutTask;

    protected override int Interval => 2700000;

    protected override int LogFrequency => 1;

    protected override Task<int> ExecuteRun()
    {
        return attendanceUpdateRepository.AutoCheckoutEveryoneByEndOfDay();
    }
}