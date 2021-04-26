using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Threading.Tasks.Task;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public class UpdateTask : IHostedService, IUpdateTask
    {
        private readonly IUpdateService _updateService;
        private readonly IConfiguration _configuration;

        private bool _taskIsActive;
        private int _successCount;
        private int _currentFailedCount;
        private DateTime? _emailSent = DateTime.UnixEpoch;
        private static readonly TimeSpan EmailSendPause = TimeSpan.FromHours(value: 1);
        private bool _successState = true;
        private readonly string _environment;
        private readonly ILogger<UpdateTask> _logger;

        public UpdateTask(IUpdateService updateService, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _updateService = updateService;
            _configuration = configuration;
            _environment = configuration.GetValue<string>(key: "Environment") ?? "unknown";
            _logger = loggerFactory.CreateLogger<UpdateTask>();
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = ExecuteAsync(cancellationToken: cancellationToken);
            return task.IsCompleted ? task : CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var activationTime = _taskIsActive ? DateTime.UtcNow : new DateTime();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_taskIsActive)
                {
                    activationTime = await WaitForActivation(cancellationToken: cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }

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
                var updateCount = await _updateService.FetchDataFromPlanningCenter().ConfigureAwait(continueOnCapturedContext: false);
                _successCount++;

                if (!_successState)
                {
                    _successState = true;
                    if (_currentFailedCount >= 10)
                    {
                        SendEmail(
                            subject: $"{_environment}: UpdateTask ran successfully", 
                            body: $"UpdateTask resumed normal operation at {DateTime.UtcNow} after {_currentFailedCount} failed executions.",
                            logger: _logger);
                    }
                    
                }

                if (_successCount % 10 == 0 || _successCount == 1)
                {
                    _updateService.LogTaskRun(success: true, updateCount: updateCount, environment: _environment);
                }
            }
            catch (Exception e)
            {
                _currentFailedCount = _successState ? 1 : ++_currentFailedCount;
                _successState = false;
                
                logger.LogError(eventId: new EventId(id: 0, name: nameof(UpdateTask)), exception: e, message: $"{_environment}: {e.Message}");

                _updateService.LogTaskRun(success: false, updateCount: 0, environment: _environment);
                
                if (_currentFailedCount % 10 == 0 && _emailSent < DateTime.UtcNow - EmailSendPause)
                {
                    SendEmail(
                        subject: $"UpdateTask failed the last {_currentFailedCount} executions: {e.Message}", 
                        body: e.Message + e.StackTrace, 
                        logger: _logger);
                    
                    _emailSent = DateTime.UtcNow;
                }
            }
        }

        private async Task<DateTime> WaitForActivation(CancellationToken cancellationToken)
        {
            while (!_taskIsActive)
            {
                await Sleep(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            return DateTime.UtcNow;
        }

        private static async Task Sleep(CancellationToken cancellationToken)
        {
            await Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            SendEmail(
                subject: $"{_environment}: System Shutdown",
                body: $"The StopAsync Method was called for UpdateTask at {DateTime.UtcNow}",
                logger: _logger);
            return CompletedTask;        
        }

        public void ActivateTask() => _taskIsActive = true;

        public void DeactivateTask() => _taskIsActive = false;

        //public bool IsTaskActive() => _taskIsActive;

        public int GetExecutionCount() => _successCount;

        private void SendEmail(string subject, string body, ILogger logger)
        {
            try
            {
                MailMessage message = new()
                {
                    Subject = $"{DateTime.UtcNow} {subject}",
                    Body = body,
                    From = new MailAddress(address: "kidstown@gvc.ch", displayName: "KidsTown")
                };

                message.To.Add(item: new MailAddress(address: "nsiddiqui@gvc.ch", displayName: "Nadina Siddiqui"));
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
                logger.LogError(eventId: new EventId(
                        id: 0, name: nameof(UpdateTask)), 
                    exception: ex, 
                    message: $"{_environment}: Sending email failed: {ex.Message}");
            }
        }
    }
}