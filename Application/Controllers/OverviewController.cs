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
    [Route(template: "[controller]")]
    public class OverviewController : ControllerBase
    {
        private readonly IOverviewService _overviewService;

        public OverviewController(IOverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        [HttpPost]
        [Route(template: "event/{eventId}/attendees")]
        [Produces(contentType: "application/json")]
        public async Task<ImmutableList<AttendeesByLocation>> GetAttendees(
            [FromRoute] long eventId, 
            [FromQuery] string date,
            [FromBody] IImmutableList<int> selectedLocationGroups)
        {
            var parsedDate = DateTime.Parse(s: date);
            var attendeesByLocation = await _overviewService.GetActiveAttendees(eventId: eventId, selectedLocationGroups: selectedLocationGroups, date: parsedDate)
                .ConfigureAwait(continueOnCapturedContext: false);
            return attendeesByLocation.OrderBy(keySelector: a => a.LocationGroupId).ThenBy(keySelector: a => a.Location).ToImmutableList();
        }
        
        [HttpPost]
        [Route(template: "event/{eventId}/attendees/headcounts")]
        [Produces(contentType: "application/json")]
        public async Task<ImmutableList<LiveHeadCounts>> GetAttendeeHeadCounts(
            [FromRoute] long eventId, 
            [FromQuery] string date,
            [FromBody] IImmutableList<int> selectedLocations)
        {
            var parsedDate = DateTime.Parse(s: date);
            var headCounts = await _overviewService.GetHeadCountsByLocations(
                    eventId: eventId,
                    selectedLocations: selectedLocations,
                    startDate: parsedDate,
                    endDate: parsedDate)
                .ConfigureAwait(continueOnCapturedContext: false);
            return headCounts;
        }
        
        [HttpPost]
        [Route(template: "event/{eventId}/attendees/history")]
        [Produces(contentType: "application/json")]
        public async Task<ImmutableList<HeadCounts>> GetAttendanceHistory(
            [FromRoute] long eventId,
            [FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetSummedUpHeadCounts(
                    eventId: eventId,
                    selectedLocationGroups: selectedLocations,
                    startDate: new DateTime())
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}