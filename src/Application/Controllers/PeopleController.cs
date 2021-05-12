using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleService _peopleService;

        public PeopleController(IPeopleService peopleService)
        {
            _peopleService = peopleService;
        }

        [HttpPost]
        [Route("adults")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAdults([FromBody] IImmutableList<int> attendanceIds)
        {
            return Ok(await _peopleService.GetParents(attendanceIds));
        }
        
        [HttpPost]
        [Route("adults/update")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateAdults([FromBody] IImmutableList<Adult> adults, [FromQuery] bool updatePhoneNumber = false)
        {
            await _peopleService.UpdateAdults(adults: adults, updatePhoneNumber: updatePhoneNumber);
            return Ok();
        }
        
        
    }
}