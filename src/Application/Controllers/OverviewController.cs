using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers;

[ApiController]
[Route(template: "[controller]/event/{eventId:long}/attendees")]
public class OverviewController : ControllerBase
{
    private readonly IOverviewService _overviewService;

    public OverviewController(IOverviewService overviewService)
    {
        _overviewService = overviewService;
    }

    [HttpPost]
    [Produces(contentType: "application/json")]
    public async Task<IImmutableList<AttendeesByLocation>> GetAttendees(
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
    [Route(template: "headcounts")]
    [Produces(contentType: "application/json")]
    public async Task<IImmutableList<LiveHeadCounts>> GetAttendeeHeadCounts(
        [FromRoute] long eventId, 
        [FromQuery] string date,
        [FromBody] IImmutableList<int> selectedLocationGroups)
    {
        var parsedDate = DateTime.Parse(s: date);
        var headCounts = await _overviewService.GetHeadCountsByLocations(
                eventId: eventId,
                selectedLocationGroups: selectedLocationGroups,
                startDate: parsedDate,
                endDate: parsedDate)
            .ConfigureAwait(continueOnCapturedContext: false);
        return headCounts;
    }
        
    [HttpPost]
    [Route(template: "history")]
    [Produces(contentType: "application/json")]
    public async Task<IImmutableList<HeadCounts>> GetAttendanceHistory(
        [FromRoute] long eventId,
        [FromBody] IImmutableList<int> selectedLocations)
    {
        return await _overviewService.GetSummedUpHeadCounts(
                eventId: eventId,
                selectedLocations: selectedLocations,
                startDate: new())
            .ConfigureAwait(continueOnCapturedContext: false);
    }
}