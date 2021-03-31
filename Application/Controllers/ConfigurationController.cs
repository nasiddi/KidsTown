using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Application.Models;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using CheckInsExtension.CheckInUpdateJobs.Update;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [Route(template: "[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly IUpdateTask _updateTask;

        public ConfigurationController(IConfigurationService configurationService, IUpdateTask updateTask)
        {
            _configurationService = configurationService;
            _updateTask = updateTask;
        }

        [HttpGet]
        [Route(template: "locations")]
        [Produces(contentType: "application/json")]
        public async Task<ImmutableList<Options>> GetLocations()
        {
            _updateTask.ActivateTask();
            var locations = await _configurationService.GetActiveLocations().ConfigureAwait(continueOnCapturedContext: false);
            return locations.Select(selector: MapOptions).ToImmutableList();
        }

        [HttpGet]
        [Route(template: "events")]
        [Produces(contentType: "application/json")]
        public async Task<ImmutableList<CheckInsEvent>> GetAvailableEvents()
        {
            return await _configurationService.GetAvailableEvents().ConfigureAwait(continueOnCapturedContext: false);
        }

        [HttpGet]
        [Route(template: "events/default")]
        [Produces(contentType: "application/json")]
        public Dictionary<string, long> GetDefaultEvent()
        {
            return new()
            {
                {"eventId", _configurationService.GetDefaultEventId()}
            };
        }
        
        private static Options MapOptions(Location location)
        {
            return new()
            {
                Value = location.Id,
                Label = location.Name
            };
        }
    }
}