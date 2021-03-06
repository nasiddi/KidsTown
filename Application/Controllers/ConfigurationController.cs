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
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly UpdateTask _updateTask;

        public ConfigurationController(IConfigurationService configurationService, UpdateTask updateTask)
        {
            _configurationService = configurationService;
            _updateTask = updateTask;
        }

        [HttpGet]
        [Route("locations")]
        [Produces("application/json")]
        public async Task<ImmutableList<Options>> GetLocations()
        {
            _updateTask.TaskIsActive = true;
            var locations = await _configurationService.GetActiveLocations();
            return locations.Select(MapOptions).ToImmutableList();
        }

        [HttpGet]
        [Route("events")]
        [Produces("application/json")]
        public async Task<ImmutableList<CheckInsEvent>> GetAvailableEvents()
        {
            return await _configurationService.GetAvailableEvents();
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