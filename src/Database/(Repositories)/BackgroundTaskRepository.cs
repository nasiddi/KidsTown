using System;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
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
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var taskExecution = new TaskExecution
            {
                InsertDate = DateTime.UtcNow,
                IsSuccess = success,
                UpdateCount = updateCount,
                Environment = environment,
                TaskName = taskName
            };

            var taskExecutionCount = await db.TaskExecutions.Where(predicate: t => t.TaskName == taskName).CountAsync();

            if (taskExecutionCount >= 100)
            {
                var toBeDeleted = taskExecutionCount - 99;

                var taskExecutionsToDelete = await db.TaskExecutions.OrderBy(keySelector: t => t.Id)
                    .Where(predicate: t => t.TaskName == taskName)
                    .Take(count: toBeDeleted).ToListAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                db.RemoveRange(entities: taskExecutionsToDelete);
            }

            await db.AddAsync(entity: taskExecution).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}