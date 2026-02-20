using System.Collections.Immutable;
using Application.Models;
using BackgroundTasks.Common;
using KidsTown;
using KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[AuthenticateUser]
[Route("api/[controller]")]
public class ConfigurationController(
        IConfigurationService configurationService,
        ITaskManagementService taskManagementService)
    : ControllerBase
{
    [HttpGet]
    [Route("location-groups")]
    [Produces("application/json")]
    public async Task<IImmutableList<SelectOption>> GetLocationGroups()
    {
        taskManagementService.ActivateBackgroundTasks();
        var locations = await configurationService.GetActiveLocationGroups();
        return locations.Select(MapOptions).ToImmutableList();
    }

    [HttpGet]
    [Route("events/{eventId:long}/location-groups/locations")]
    [Produces("application/json")]
    public async Task<IImmutableList<GroupedSelectOptions>> GetLocationsByLocationGroups([FromRoute] long eventId)
    {
        var locations = await configurationService.GetLocations(eventId);

        return locations.GroupBy(l => l.LocationGroupId)
            .Select(
                g => new GroupedSelectOptions
                {
                    GroupId = g.Key,
                    Options = g.Select(MapOptions).ToImmutableList()
                })
            .ToImmutableList();
    }

    [HttpGet]
    [Route("events")]
    [Produces("application/json")]
    public async Task<IImmutableList<CheckInsEvent>> GetAvailableEvents()
    {
        return await configurationService.GetAvailableEvents();
    }

    [HttpGet]
    [Route("events/default")]
    [Produces("application/json")]
    public Dictionary<string, long> GetDefaultEvent()
    {
        return new Dictionary<string, long>
        {
            {"eventId", configurationService.GetDefaultEventId()}
        };
    }

    private static SelectOption MapOptions(LocationGroup locationGroup)
    {
        return new SelectOption
        {
            Value = locationGroup.Id,
            Label = locationGroup.Name
        };
    }

    private static SelectOption MapOptions(Location location)
    {
        return new SelectOption
        {
            Value = location.Id,
            Label = location.Name
        };
    }

    [HttpGet]
    [Route("tasks")]
    [Produces("application/json")]
    public IImmutableList<TaskOverview> GetBackgroundTasks()
    {
        return taskManagementService.GetTaskOverviews();
    }
}