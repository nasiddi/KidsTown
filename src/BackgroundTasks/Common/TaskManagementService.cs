using System;
using System.Collections.Immutable;
using System.Linq;

namespace KidsTown.BackgroundTasks.Common
{
    public class TaskManagementService : ITaskManagementService
    {
        private readonly Func<BackgroundTaskType, IBackgroundTask> _serviceResolver;

        public TaskManagementService(Func<BackgroundTaskType, IBackgroundTask> serviceResolver)
        {
            _serviceResolver = serviceResolver;
        }

        public void ActivateBackgroundTasks()
        {
            var tasks = GetAllTasks();

            tasks.ForEach(action: t =>
            {
                t.ActivateTask();
            });
        }

        public IImmutableList<TaskOverview> GetTaskOverviews()
        {
            return GetAllTasks().Select(selector: t => new TaskOverview
            {
                BackgroundTaskType = t.GetBackgroundTaskType(),
                IsActive = t.IsTaskActive(),
                TaskRunsSuccessfully = t.TaskRunsSuccessfully(),
                SuccessCount = t.GetExecutionCount(),
                CurrentFailCount = t.GetCurrentFailCount(),
                Interval = t.GetInterval(),
                LogFrequency = t.GetLogFrequency(),
                LastExecution = t.GetLastExecution()
            }).ToImmutableList();
        }
        
        private ImmutableList<IBackgroundTask> GetAllTasks()
        {
            var tasks = Enum.GetValues(enumType: typeof(BackgroundTaskType))
                .Cast<BackgroundTaskType>()
                .ToList()
                .Select(selector: t => _serviceResolver(arg: t))
                .ToImmutableList();
            return tasks;
        }

        
    }
}