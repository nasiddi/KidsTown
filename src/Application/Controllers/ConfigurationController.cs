using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.Common;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ITaskManagementService _taskManagementService;

        public ConfigurationController(IConfigurationService configurationService, ITaskManagementService taskManagementService)
        {
            _configurationService = configurationService;
            _taskManagementService = taskManagementService;
        }

        [HttpGet]
        [Route("location-groups")]
        [Produces("application/json")]
        public async Task<IImmutableList<SelectOption>> GetLocationGroups()
        {
            _taskManagementService.ActivateBackgroundTasks();
            var locations = await _configurationService.GetActiveLocationGroups().ConfigureAwait(false);
            return locations.Select(MapOptions).ToImmutableList();
        }
        
        [HttpPost]
        [Route("events/{eventId:long}/location-groups/locations")]
        [Produces("application/json")]
        public async Task<IImmutableList<GroupedSelectOptions>> GetLocationsByLocationGroups(
            [FromRoute] long eventId,
            [FromBody] IImmutableList<int> selectedLocationGroups)
        {
            var locations = await _configurationService.GetLocations(eventId: eventId, selectedLocationGroups: selectedLocationGroups)
                .ConfigureAwait(false);

            return locations.GroupBy(l => l.LocationGroupId)
                .Select(g => new GroupedSelectOptions
                {
                    GroupId = g.Key,
                    Options = g.Select(MapOptions).ToImmutableList(),
                    OptionCount = g.Count()
                }).ToImmutableList();
        }
        
        [HttpGet]
        [Route("events/{eventId:long}/locations")]
        [Produces("application/json")]
        public async Task<IImmutableList<SelectOption>> GetLocations([FromRoute] long eventId)
        {
            var locations = await _configurationService.GetLocations(eventId: eventId, selectedLocationGroups: null)
                .ConfigureAwait(false);

            return locations.Select(MapOptions).ToImmutableList();
        }

        [HttpGet]
        [Route("events")]
        [Produces("application/json")]
        public async Task<IImmutableList<CheckInsEvent>> GetAvailableEvents()
        {
            return await _configurationService.GetAvailableEvents().ConfigureAwait(false);
        }

        [HttpGet]
        [Route("events/default")]
        [Produces("application/json")]
        public Dictionary<string, long> GetDefaultEvent()
        {
            return new()
            {
                {"eventId", _configurationService.GetDefaultEventId()}
            };
        }
        
        private static SelectOption MapOptions(LocationGroup locationGroup)
        {
            return new()
            {
                Value = locationGroup.Id,
                Label = locationGroup.Name
            };
        }
        
        private static SelectOption MapOptions(Location location)
        {
            return new()
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
            return _taskManagementService.GetTaskOverviews();
        }
    }
}