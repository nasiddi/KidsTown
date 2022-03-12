using System.Collections.Immutable;

namespace KidsTown.BackgroundTasks.Common;

public interface ITaskManagementService
{
    void ActivateBackgroundTasks();
    IImmutableList<TaskOverview> GetTaskOverviews();
}