using System;
using System.Diagnostics;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        private bool _taskIsActive;
        private int _executionCount;
        private DateTime? _emailSent = DateTime.UnixEpoch;
        private static readonly TimeSpan EmailSendPause = TimeSpan.FromHours(value: 1);
        private bool _successState = true;
        private readonly string _environment;

        public UpdateTask(IUpdateService updateService, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _updateService = updateService;
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _environment = configuration.GetValue<string>(key: "Environment") ?? "unknown";
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = ExecuteAsync(cancellationToken: cancellationToken);
            return task.IsCompleted ? task : CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var logger = _loggerFactory.CreateLogger<UpdateTask>();
            var activationTime = _taskIsActive ? DateTime.UtcNow : new DateTime();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_taskIsActive)
                {
                    activationTime = await WaitForActivation(cancellationToken: cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }

                await RunTask(logger: logger).ConfigureAwait(continueOnCapturedContext: false);

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
                _executionCount++;

                if (!_successState)
                {
                    _successState = true;
                    SendEmail(
                        subject: "UpdateTask ran sucessfully", 
                        body: $"UpdateTask resumed normal operation at {DateTime.UtcNow}");
                }

                if (_executionCount % 10 == 0 || _executionCount == 1)
                {
                    _updateService.LogTaskRun(success: true, updateCount: updateCount, environment: _environment);
                }
            }
            catch (Exception e)
            {
                _successState = false;
                logger.LogError(eventId: new EventId(id: 0, name: nameof(UpdateTask)), exception: e, message: e.Message);

                _updateService.LogTaskRun(success: false, updateCount: 0, environment: _environment);
                
                if (_emailSent < DateTime.UtcNow - EmailSendPause)
                {
                    try
                    {
                        SendEmail(subject: $"UpdateTask failed: {e.Message}", body: e.Message + e.StackTrace);
                        _emailSent = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(eventId: new EventId(
                            id: 0, name: nameof(UpdateTask)), 
                            exception: ex, 
                            message: $"Sending email failed: {ex.Message}");
                    }
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
                subject: "System Shutdown",
                body: $"The StopAsync Method was called for UpdateTask at {DateTime.UtcNow}");
            return CompletedTask;        
        }

        public void ActivateTask() => _taskIsActive = true;

        public void DeactivateTask() => _taskIsActive = false;

        //public bool IsTaskActive() => _taskIsActive;

        public int GetExecutionCount() => _executionCount;

        private void SendEmail(string subject, string body)
        {
            MailMessage message = new()
            {
                Subject = $"{DateTime.UtcNow} {subject}",
                Body = body,
                From = new MailAddress(address: "kidstown@gvc.ch", displayName: "KidsTown")
            };

            message.To.Add(new MailAddress(address: "nsiddiqui@gvc.ch", displayName: "Nadina Siddiqui"));
            var username = _configuration.GetValue<string>(key: "MailAccount:Username");
            var password = _configuration.GetValue<string>(key: "MailAccount:Password");
            
            
            SmtpClient client = new()
            {
                Host = "smtp.office365.com",
                Credentials = new NetworkCredential(username, password),
                Port = 587,
                EnableSsl = true,
            };

            try
            {
                client.Send(message: message); 
            }
            catch (Exception ex)
            {
                Trace.WriteLine(message: ex.ToString());
            }
        }
    }
}