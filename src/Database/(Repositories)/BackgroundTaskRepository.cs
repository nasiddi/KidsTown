using System;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.Database.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Database
{
    public class BackgroundTaskRepository : IBackgroundTaskRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BackgroundTaskRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task LogTaskRun(bool success, int updateCount, string environment, string taskName)
        {
            await using var db = CommonRepository.GetDatabase(_serviceScopeFactory);

            var taskExecution = new TaskExecution
            {
                InsertDate = DateTime.UtcNow,
                IsSuccess = success,
                UpdateCount = updateCount,
                Environment = environment,
                TaskName = taskName
            };

            var taskExecutionCount = await db.TaskExecutions.Where(t => t.TaskName == taskName && t.Environment == environment).CountAsync();

            if (taskExecutionCount >= 100)
            {
                var toBeDeleted = taskExecutionCount - 99;

                var taskExecutionsToDelete = await db.TaskExecutions.OrderBy(t => t.Id)
                    .Where(t => t.TaskName == taskName)
                    .Take(toBeDeleted).ToListAsync()
                    .ConfigureAwait(false);

                db.RemoveRange(taskExecutionsToDelete);
            }

            await db.AddAsync(taskExecution).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}