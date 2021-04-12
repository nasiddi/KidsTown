using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Email;
using Aspose.Email.Clients;
using Aspose.Email.Clients.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Threading.Tasks.Task;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public class UpdateTask : IHostedService, IUpdateTask
    {
        private readonly IUpdateService _updateService;
        private readonly ILoggerFactory _loggerFactory;

        private bool _taskIsActive = true;
        private int _executionCount;
        private DateTime? _emailSent = DateTime.UnixEpoch;
        private static readonly TimeSpan EmailSendPause = TimeSpan.FromHours(value: 1);
        private bool _successState = true;

        public UpdateTask(IUpdateService updateService, ILoggerFactory loggerFactory)
        {
            _updateService = updateService;
            _loggerFactory = loggerFactory;
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
                await _updateService.FetchDataFromPlanningCenter().ConfigureAwait(continueOnCapturedContext: false);
                _executionCount++;

                if (!_successState)
                {
                    _successState = true;
                    SendEmail(
                        subject: "UpdateTask ran sucessfully", 
                        body: $"UpdateTask resumed normal operation at {DateTime.UtcNow}");
                }
            }
            catch (Exception e)
            {
                _successState = false;
                logger.LogError(eventId: new EventId(id: 0, name: nameof(UpdateTask)), exception: e, message: e.Message);

                if (_emailSent < DateTime.UtcNow - EmailSendPause)
                {
                    SendEmail(subject: $"UpdateTask failed {e.Message}", body: e.Message + e.StackTrace);
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
            return CompletedTask;        
        }

        public void ActivateTask() => _taskIsActive = true;

        public void DeactivateTask() => _taskIsActive = false;

        //public bool IsTaskActive() => _taskIsActive;

        public int GetExecutionCount() => _executionCount;

        private static void SendEmail(string subject, string body)
        {
            MailMessage message = new()
            {
                Subject = $"{DateTime.UtcNow} {subject}",
                Body = body,
                From = new MailAddress(address: "kidstown@gvc.ch", displayName: "KidsTown", ignoreSmtpCheck: false)
            };

            message.To.Add(address: new MailAddress(address: "nsiddiqui@gvc.ch", displayName: "Nadina Siddiqui", ignoreSmtpCheck: false));

            SmtpClient client = new()
            {
                Host = "smtp.office365.com",
                Username = "kidstown@gvc.ch",
                Password = "Jobarena.21",
                Port = 587,
                SecurityOptions = SecurityOptions.SSLExplicit
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