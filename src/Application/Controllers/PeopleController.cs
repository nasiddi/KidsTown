using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleRepository _peopleRepository;

        public PeopleController(IPeopleRepository peopleRepository)
        {
            _peopleRepository = peopleRepository;
        }

        [HttpPost]
        [Route("adults")]
        [Produces("application/json")]
        public async Task<IActionResult> GetPeople([FromBody] IImmutableList<int> attendanceIds)
        {
            return Ok(await _peopleRepository.GetParents(attendanceIds));
        }
    }
}