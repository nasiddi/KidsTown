using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Threading.Tasks.Task;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class UpdateTask : IHostedService
    {
        private readonly ILogger<UpdateTask> _logger;
        private readonly IUpdateService _updateService;

        public bool TaskIsActive { get; set; } = true;

        public UpdateTask(ILogger<UpdateTask> logger, IUpdateService updateService)
        {
            _logger = logger;
            _updateService = updateService;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var activationTime = new DateTime();
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!TaskIsActive)
                {
                    while (!TaskIsActive)
                    {
                        await Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);
                    }
                    activationTime = DateTime.UtcNow;
                }

                try
                {
                    await _updateService.FetchDataFromPlanningCenter();
                }
                catch (Exception e)
                {
                    _logger.LogError(message: e.Message, e);
                }

                await Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);

                if (activationTime < DateTime.UtcNow.Date.AddHours(value: 1))
                {
                    TaskIsActive = false;
                }
            }
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(message: "Timed Hosted Service is stopping.");
            return CompletedTask;        
        }
    }
}