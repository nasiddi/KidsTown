using System;

namespace KidsTown.BackgroundTasks.Common;

public interface IBackgroundTask
{
    void ActivateTask();
    bool IsTaskActive();
    int GetExecutionCount();
    int GetCurrentFailCount();
    bool TaskRunsSuccessfully();
    BackgroundTaskType GetBackgroundTaskType();
    int GetInterval();
    int GetLogFrequency();
    DateTime? GetLastExecution();
}