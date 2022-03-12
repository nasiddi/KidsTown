using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.BackgroundTasks.Common;

public class TaskOverview
{
    public BackgroundTaskType BackgroundTaskType { get; init; }
    public bool IsActive { get; init; }
    public bool TaskRunsSuccessfully { get; init; }
    public int SuccessCount { get; init; }
    public int CurrentFailCount { get; init; }
    public int Interval { get; init; }
    public int LogFrequency { get; init; }
    public DateTime? LastExecution { get; init; }
}