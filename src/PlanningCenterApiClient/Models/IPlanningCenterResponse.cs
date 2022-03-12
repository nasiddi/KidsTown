using System;

namespace KidsTown.PlanningCenterApiClient.Models;

public interface IPlanningCenterResponse
{
    public Uri? NextLink { get; }
}