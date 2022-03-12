using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KidsTown.BackgroundTasks.Common;

public abstract class BackgroundTask : IHostedService, IBackgroundTask
{
    protected const int DaysLookBack = 7;

    protected abstract BackgroundTaskType BackgroundTaskType { get; }
    protected abstract int Interval { get; }
    protected abstract int LogFrequency { get; }
        
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackgroundTask> _logger;
    private readonly string _environment;

    private static readonly TimeSpan EmailSendPause = TimeSpan.FromHours(value: 1);
        
    private bool _taskIsActive;
    private bool _taskRunsSuccessfully = true;
        
    private int _successCount;
    private int _currentFailedCount;
        
    private DateTime _emailSent = DateTime.UnixEpoch;
    private DateTime? _lastExecution;

    protected BackgroundTask(IBackgroundTaskRepository backgroundTaskRepository, ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _backgroundTaskRepository = backgroundTaskRepository;
        _configuration = configuration;
        _environment = configuration.GetValue<string>(key: "Environment") ?? "unknown";
        _logger = loggerFactory.CreateLogger<BackgroundTask>();
    }
        
    public void ActivateTask()
    {
        _taskIsActive = true;
    }
        
    public bool TaskRunsSuccessfully() => _taskRunsSuccessfully;
        
    public void DeactivateTask() => _taskIsActive = false;

    public bool IsTaskActive() => _taskIsActive;

    public int GetExecutionCount() => _successCount;
    public int GetCurrentFailCount() => _currentFailedCount;
    public BackgroundTaskType GetBackgroundTaskType() => BackgroundTaskType;
    public int GetInterval() => Interval;
    public int GetLogFrequency() => LogFrequency;
    public DateTime? GetLastExecution() => _lastExecution;
        
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var task = ExecuteAsync(cancellationToken: cancellationToken);
        return task.IsCompleted ? task : Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        SendEmail(
            subject: $"{GetSubjectPrefix()} System Shutdown",
            body: $"The StopAsync Method was called for UpdateTask at {DateTime.UtcNow}",
            logger: _logger);
        return Task.CompletedTask;        
    }
        
    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime? activationTime = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            activationTime = await WaitForActivation(activationTime: activationTime, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            await RunTask(logger: _logger).ConfigureAwait(continueOnCapturedContext: false);
            await Sleep(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            if (activationTime < DateTime.UtcNow.Date.AddHours(value: 1))
            {
                _taskIsActive = false;
            }
        }
    }

    private async Task RunTask(ILogger logger)
    {
        try
        {
            _lastExecution = DateTime.UtcNow;
            var updateCount = await ExecuteRun().ConfigureAwait(continueOnCapturedContext: false);
            _successCount++;

            if (!_taskRunsSuccessfully)
            {
                _taskRunsSuccessfully = true;
                if (_currentFailedCount >= 5)
                {
                    SendEmail(
                        subject: $"{GetSubjectPrefix()} Task ran successfully",
                        body: $"Task resumed normal operation at {DateTime.UtcNow} after {_currentFailedCount} failed executions.",
                        logger: _logger);
                }
            }

            if (_successCount % LogFrequency == 0 || _successCount == 1)
            {
                LogTaskRun(success: true, updateCount: updateCount, environment: _environment);
            }
        }
        catch (Exception e)
        {
            LogException(logger: logger, exception: e);
        }
    }

    private void LogException(ILogger logger, Exception exception)
    {
        _currentFailedCount = _taskRunsSuccessfully ? 1 : ++_currentFailedCount;
        _taskRunsSuccessfully = false;

        logger.LogError(eventId: new(id: 0, name: nameof(BackgroundTask)), exception: exception,
            message: $"{GetSubjectPrefix()} {exception.Message}");

        LogTaskRun(success: false, updateCount: 0, environment: _environment);

        if (_currentFailedCount % 5 != 0 || _emailSent >= DateTime.UtcNow - EmailSendPause)
        {
            return;
        }
            
        SendEmail(
            subject: $"{GetSubjectPrefix()} Task failed the last {_currentFailedCount} executions.",
            body: exception.Message + exception.StackTrace,
            logger: _logger);

        _emailSent = DateTime.UtcNow;
    }

    private string GetSubjectPrefix()
    {
        return $"{_environment}, {BackgroundTaskType.ToString()}:";
    }

    protected abstract Task<int> ExecuteRun();

    private async Task<DateTime> WaitForActivation(DateTime? activationTime, CancellationToken cancellationToken)
    {
        var waitTask = Task.Run(function: async () =>
        {
            while (!_taskIsActive)
            {
                await Task.Delay(millisecondsDelay: 1000, cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }, cancellationToken: cancellationToken);

        await Task.WhenAny(task1: waitTask, task2: Task.Delay(millisecondsDelay: -1, cancellationToken: cancellationToken));

        return activationTime ?? DateTime.UtcNow;
    }

    private async Task Sleep(CancellationToken cancellationToken)
    {
        await Task.Delay(millisecondsDelay: Interval, cancellationToken: cancellationToken)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private void LogTaskRun(bool success, int updateCount, string environment)
    {
        _backgroundTaskRepository.LogTaskRun(
            success: success,
            updateCount: updateCount,
            environment: environment,
            taskName: BackgroundTaskType.ToString());
    }
        
    private void SendEmail(string subject, string body, ILogger logger)
    {
        try
        {
            MailMessage message = new()
            {
                Subject = $"{DateTime.UtcNow} {subject}",
                Body = body,
                From = new(address: "kidstown@gvc.ch", displayName: "KidsTown")
            };

            message.To.Add(item: new(address: "nadinasiddiqui@msn.com", displayName: "Nadina Siddiqui"));
            var username = _configuration.GetValue<string>(key: "MailAccount:Username");
            var password = _configuration.GetValue<string>(key: "MailAccount:Password");


            SmtpClient client = new()
            {
                Host = "smtp.office365.com",
                Credentials = new NetworkCredential(userName: username, password: password),
                Port = 587,
                EnableSsl = true
            };
            client.Send(message: message);
        }
        catch (Exception ex)
        {
            logger.LogError(eventId: new(
                    id: 0, name: nameof(BackgroundTask)), 
                exception: ex, 
                message: $"{_environment}: Sending email failed: {ex.Message}");
        }
    }
}