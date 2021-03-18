using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using static System.Threading.Tasks.Task;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class UpdateTask : IHostedService
    {
        private readonly IUpdateService _updateService;

        public bool TaskIsActive { get; set; } = true;
        public int ExecutionCount { get; private set; }

        public UpdateTask(IUpdateService updateService)
        {
            _updateService = updateService;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var task = ExecuteAsync(cancellationToken: cancellationToken);
            return task.IsCompleted ? task : CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var activationTime = TaskIsActive ? DateTime.UtcNow : new DateTime();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!TaskIsActive)
                {
                    activationTime = await WaitForActivation(cancellationToken: cancellationToken)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }

                await RunTask().ConfigureAwait(continueOnCapturedContext: false);

                await Sleep(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                if (activationTime < DateTime.UtcNow.Date.AddHours(value: 1))
                {
                    TaskIsActive = false;
                }
            }
        }

        private async Task RunTask()
        {
            try
            {
                await _updateService.FetchDataFromPlanningCenter().ConfigureAwait(continueOnCapturedContext: false);
                ExecutionCount++;
            }
            catch (Exception e)
            {
                Console.WriteLine(value: e.Message);
                Console.WriteLine(value: e.StackTrace);
            }
        }

        private async Task<DateTime> WaitForActivation(CancellationToken cancellationToken)
        {
            while (!TaskIsActive)
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
    }
}