using System.Collections.Immutable;

namespace BackgroundTasks.Common;

public interface ITaskManagementService
{
    void ActivateBackgroundTasks();
    IImmutableList<TaskOverview> GetTaskOverviews();
}