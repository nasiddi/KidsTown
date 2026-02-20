using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundTasks.Common;

public abstract class BackgroundTask(
        IBackgroundTaskRepository backgroundTaskRepository,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    : IHostedService, IBackgroundTask
{
    protected const int DaysLookBack = 7;

    private static readonly TimeSpan EmailSendPause = TimeSpan.FromHours(value: 1);
    private readonly string environment = configuration.GetValue<string>("Environment") ?? "unknown";

    private readonly ILogger<BackgroundTask> logger = loggerFactory.CreateLogger<BackgroundTask>();
    private int currentFailedCount;

    private DateTime emailSent = DateTime.UnixEpoch;
    private DateTime? lastExecution;

    private int successCount;

    private bool taskIsActive;
    private bool taskRunsSuccessfully = true;

    protected abstract BackgroundTaskType BackgroundTaskType { get; }

    protected abstract int Interval { get; }

    protected abstract int LogFrequency { get; }

    public void ActivateTask()
    {
        taskIsActive = true;
    }

    public bool TaskRunsSuccessfully()
    {
        return taskRunsSuccessfully;
    }

    public bool IsTaskActive()
    {
        return taskIsActive;
    }

    public int GetExecutionCount()
    {
        return successCount;
    }

    public int GetCurrentFailCount()
    {
        return currentFailedCount;
    }

    public BackgroundTaskType GetBackgroundTaskType()
    {
        return BackgroundTaskType;
    }

    public int GetInterval()
    {
        return Interval;
    }

    public int GetLogFrequency()
    {
        return LogFrequency;
    }

    public DateTime? GetLastExecution()
    {
        return lastExecution;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var task = ExecuteAsync(cancellationToken);
        return task.IsCompleted ? task : Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        SendEmail(
            $"{GetSubjectPrefix()} System Shutdown",
            $"The StopAsync Method was called for UpdateTask at {DateTime.UtcNow}",
            logger);

        return Task.CompletedTask;
    }

    public void DeactivateTask()
    {
        taskIsActive = false;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime? activationTime = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            activationTime = await WaitForActivation(activationTime, cancellationToken);
            await RunTask(logger);
            await Sleep(cancellationToken);

            if (activationTime < DateTime.UtcNow.Date.AddHours(value: 1))
            {
                taskIsActive = false;
            }
        }
    }

    private async Task RunTask(ILogger logger)
    {
        try
        {
            lastExecution = DateTime.UtcNow;
            var updateCount = await ExecuteRun();
            successCount++;

            if (!taskRunsSuccessfully)
            {
                taskRunsSuccessfully = true;
                if (currentFailedCount >= 5)
                {
                    SendEmail(
                        $"{GetSubjectPrefix()} Task ran successfully",
                        $"Task resumed normal operation at {DateTime.UtcNow} after {currentFailedCount} failed executions.",
                        this.logger);
                }
            }

            if (successCount % LogFrequency == 0 || successCount == 1)
            {
                LogTaskRun(success: true, updateCount, environment);
            }
        }
        catch (Exception e)
        {
            LogException(logger, e);
        }
    }

    private void LogException(ILogger logger, Exception exception)
    {
        currentFailedCount = taskRunsSuccessfully ? 1 : ++currentFailedCount;
        taskRunsSuccessfully = false;

        logger.LogError(
            new EventId(id: 0, nameof(BackgroundTask)),
            exception,
            $"{GetSubjectPrefix()} {exception.Message}");

        LogTaskRun(success: false, updateCount: 0, environment);

        if (currentFailedCount % 5 != 0 || emailSent >= DateTime.UtcNow - EmailSendPause)
        {
            return;
        }

        SendEmail(
            $"{GetSubjectPrefix()} Task failed the last {currentFailedCount} executions.",
            exception.Message + exception.StackTrace,
            this.logger);

        emailSent = DateTime.UtcNow;
    }

    private string GetSubjectPrefix()
    {
        return $"{environment}, {BackgroundTaskType.ToString()}:";
    }

    protected abstract Task<int> ExecuteRun();

    private async Task<DateTime> WaitForActivation(DateTime? activationTime, CancellationToken cancellationToken)
    {
        var waitTask = Task.Run(
            async () =>
            {
                while (!taskIsActive)
                {
                    await Task.Delay(millisecondsDelay: 1000, cancellationToken);
                }
            },
            cancellationToken);

        await Task.WhenAny(waitTask, Task.Delay(millisecondsDelay: -1, cancellationToken));

        return activationTime ?? DateTime.UtcNow;
    }

    private async Task Sleep(CancellationToken cancellationToken)
    {
        await Task.Delay(Interval, cancellationToken);
    }

    private void LogTaskRun(bool success, int updateCount, string environment)
    {
        backgroundTaskRepository.LogTaskRun(
            success,
            updateCount,
            environment,
            BackgroundTaskType.ToString());
    }

    private void SendEmail(string subject, string body, ILogger logger)
    {
        try
        {
            MailMessage message = new()
            {
                Subject = $"{DateTime.UtcNow} {subject}",
                Body = body,
                From = new MailAddress("kidstown@gvc.ch", "KidsTown")
            };

            message.To.Add(new MailAddress("nadinasiddiqui@msn.com", "Nadina Siddiqui"));
            var username = configuration.GetValue<string>("MailAccount:Username");
            var password = configuration.GetValue<string>("MailAccount:Password");

            SmtpClient client = new()
            {
                Host = "smtp.office365.com",
                Credentials = new NetworkCredential(username, password),
                Port = 587,
                EnableSsl = true
            };

            client.Send(message);
        }
        catch (Exception ex)
        {
            logger.LogError(
                new EventId(id: 0, nameof(BackgroundTask)),
                ex,
                $"{environment}: Sending email failed: {ex.Message}");
        }
    }
}