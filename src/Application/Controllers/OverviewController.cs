using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers
{
    [ApiController]
    [Route("[controller]/event/{eventId:long}/attendees")]
    public class OverviewController : ControllerBase
    {
        private readonly IOverviewService _overviewService;

        public OverviewController(IOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IImmutableList<AttendeesByLocation>> GetAttendees(
            [FromRoute] long eventId, 
            [FromQuery] string date,
            [FromBody] IImmutableList<int> selectedLocationGroups)
        {
            var parsedDate = DateTime.Parse(date);
            var attendeesByLocation = await _overviewService.GetActiveAttendees(eventId: eventId, selectedLocationGroups: selectedLocationGroups, date: parsedDate)
                .ConfigureAwait(false);
            return attendeesByLocation.OrderBy(a => a.LocationGroupId).ThenBy(a => a.Location).ToImmutableList();
        }
        
        [HttpPost]
        [Route("headcounts")]
        [Produces("application/json")]
        public async Task<IImmutableList<LiveHeadCounts>> GetAttendeeHeadCounts(
            [FromRoute] long eventId, 
            [FromQuery] string date,
            [FromBody] IImmutableList<int> selectedLocationGroups)
        {
            var parsedDate = DateTime.Parse(date);
            var headCounts = await _overviewService.GetHeadCountsByLocations(
                    eventId: eventId,
                    selectedLocationGroups: selectedLocationGroups,
                    startDate: parsedDate,
                    endDate: parsedDate)
                .ConfigureAwait(false);
            return headCounts;
        }
        
        [HttpPost]
        [Route("history")]
        [Produces("application/json")]
        public async Task<IImmutableList<HeadCounts>> GetAttendanceHistory(
            [FromRoute] long eventId,
            [FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetSummedUpHeadCounts(
                    eventId: eventId,
                    selectedLocations: selectedLocations,
                    startDate: new DateTime())
                .ConfigureAwait(false);
        }
    }
}