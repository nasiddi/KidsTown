using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers;

[ApiController]
[AuthenticateUser]
[Route("[controller]/adults")]
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