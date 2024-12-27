using System;
using System.Collections.Immutable;
using System.Linq;

namespace KidsTown.BackgroundTasks.Common;

public class TaskManagementService(Func<BackgroundTaskType, IBackgroundTask> serviceResolver) : ITaskManagementService
{
    public void ActivateBackgroundTasks()
    {
        var tasks = GetAllTasks();

        tasks.ForEach(t => { t.ActivateTask(); });
    }

    public IImmutableList<TaskOverview> GetTaskOverviews()
    {
        return GetAllTasks()
            .Select(
                t => new TaskOverview
                {
                    BackgroundTaskType = t.GetBackgroundTaskType(),
                    IsActive = t.IsTaskActive(),
                    TaskRunsSuccessfully = t.TaskRunsSuccessfully(),
                    SuccessCount = t.GetExecutionCount(),
                    CurrentFailCount = t.GetCurrentFailCount(),
                    Interval = t.GetInterval(),
                    LogFrequency = t.GetLogFrequency(),
                    LastExecution = t.GetLastExecution()
                })
            .ToImmutableList();
    }

    private ImmutableList<IBackgroundTask> GetAllTasks()
    {
        var tasks = Enum.GetValues(typeof(BackgroundTaskType))
            .Cast<BackgroundTaskType>()
            .ToList()
            .Select(t => serviceResolver(t))
            .ToImmutableList();

        return tasks;
    }
}