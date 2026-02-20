using System.Collections.Immutable;
using KidsTown;
using KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[ApiController]
[AuthenticateUser]
[Route("api/[controller]/adults")]
public class PeopleController(IPeopleService peopleService) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public async Task<IActionResult> GetAdults([FromBody] IImmutableList<int> attendanceIds)
    {
        return Ok(await peopleService.GetParents(attendanceIds));
    }

    [HttpPost]
    [Route("update")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateAdults([FromBody] IImmutableList<Adult> adults, [FromQuery] bool? updatePhoneNumber)
    {
        await peopleService.UpdateAdults(adults, updatePhoneNumber ?? false);
        return Ok();
    }
}