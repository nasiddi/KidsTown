using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OverviewController : ControllerBase
    {
        private readonly IOverviewService _overviewService;

        public OverviewController(IOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        [HttpPost]
        [Route("event/{eventId}/attendees/active")]
        [Produces("application/json")]
        public async Task<ImmutableList<Attendee>> GetActiveAttendees(
            [FromRoute] long eventId, 
            [FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetActiveAttendees(eventId, selectedLocations);
        }
        
        [HttpPost]
        [Route("event/{eventId}/attendees/history")]
        [Produces("application/json")]
        public async Task<ImmutableList<DailyStatistic>> GetAttendanceHistory(
            [FromRoute] long eventId,
            [FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetAttendanceHistory(eventId, selectedLocations);
        }
    }
}