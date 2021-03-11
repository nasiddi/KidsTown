using System;
using System.Collections.Immutable;
using System.Linq;
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
        [Route("event/{eventId}/attendees")]
        [Produces("application/json")]
        public async Task<ImmutableList<AttendeesByLocation>> GetAttendees(
            [FromRoute] long eventId, 
            [FromQuery] string date,
            [FromBody] IImmutableList<int> selectedLocationGroups)
        {
            var parsedDate = DateTime.Parse(date);
            var attendeesByLocation = await _overviewService.GetActiveAttendees(eventId, selectedLocationGroups, parsedDate);
            return attendeesByLocation.OrderBy(a => a.LocationGroupId).ThenBy(a => a.Location).ToImmutableList();
        }
        
        [HttpPost]
        [Route("event/{eventId}/attendees/headcounts")]
        [Produces("application/json")]
        public async Task<ImmutableList<HeadCounts>> GetAttendeeHeadCounts(
            [FromRoute] long eventId, 
            [FromQuery] string date,
            [FromBody] IImmutableList<int> selectedLocations)
        {
            var parsedDate = DateTime.Parse(date);
            var headCounts = await _overviewService.GetHeadCountsByLocations(eventId, selectedLocations, parsedDate, parsedDate);
            return headCounts;
        }
        
        [HttpPost]
        [Route("event/{eventId}/attendees/history")]
        [Produces("application/json")]
        public async Task<ImmutableList<HeadCounts>> GetAttendanceHistory(
            [FromRoute] long eventId,
            [FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetSummedUpHeadCounts(eventId, selectedLocations, new DateTime());
        }
    }
}