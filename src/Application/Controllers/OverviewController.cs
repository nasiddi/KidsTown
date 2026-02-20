using System.Collections.Immutable;
using KidsTown;
using KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[AuthenticateUser]
[Route("api/[controller]/event/{eventId:long}/attendees")]
public class OverviewController(IOverviewService overviewService) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public async Task<IImmutableList<AttendeesByLocation>> GetAttendees(
        [FromRoute] long eventId,
        [FromQuery] string date,
        [FromBody] IImmutableList<int> selectedLocationGroups)
    {
        var parsedDate = DateTime.Parse(date);
        var attendeesByLocation = await overviewService.GetActiveAttendees(eventId, selectedLocationGroups, parsedDate);

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
        var headCounts = await overviewService.GetHeadCountsByLocations(
                eventId,
                selectedLocationGroups,
                parsedDate,
                parsedDate);

        return headCounts;
    }

    [HttpPost]
    [Route("history")]
    [Produces("application/json")]
    public async Task<IImmutableList<HeadCounts>> GetAttendanceHistory(
        [FromRoute] long eventId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromBody] IImmutableList<int> selectedLocations)
    {
        return await overviewService.GetSummedUpHeadCounts(
                eventId,
                selectedLocations,
                startDate,
                endDate);
    }
}