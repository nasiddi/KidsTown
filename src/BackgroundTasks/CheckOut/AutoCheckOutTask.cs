using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.CheckOut;

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