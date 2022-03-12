using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers;

[ApiController]
[Route(template: "[controller]/adults")]
public class PeopleController : ControllerBase
{
    private readonly IPeopleService _peopleService;

    public PeopleController(IPeopleService peopleService)
    {
        _peopleService = peopleService;
    }

    [HttpPost]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> GetAdults([FromBody] IImmutableList<int> attendanceIds)
    {
        return Ok(value: await _peopleService.GetParents(attendanceIds: attendanceIds));
    }
        
    [HttpPost]
    [Route(template: "update")]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> UpdateAdults([FromBody] IImmutableList<Adult> adults, [FromQuery] bool? updatePhoneNumber)
    {
        await _peopleService.UpdateAdults(adults: adults, updatePhoneNumber: updatePhoneNumber ?? false);
        return Ok();
    }
        
        
}