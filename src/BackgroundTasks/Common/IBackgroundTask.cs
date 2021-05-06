using System;

namespace KidsTown.BackgroundTasks.Common
{
    public interface IBackgroundTask
    {
        void ActivateTask();
        //public void DeactivateTask();
        bool IsTaskActive();
        int GetExecutionCount();
        int GetCurrentFailCount();
        void DisableTask();
        void EnableTask();
        bool IsEnabled();
        bool TaskRunsSuccessfully();
        BackgroundTaskType GetBackgroundTaskType();
        int GetInterval();
        int GetLogFrequency();
        DateTime? GetLastExecution();
    }
}