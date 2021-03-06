using System.Collections.Immutable;
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
        [Route("attendees/active")]
        [Produces("application/json")]
        public async Task<ImmutableList<Attendee>> GetActiveAttendees([FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetActiveAttendees(selectedLocations);
        }
        
        [HttpPost]
        [Route("attendees/history")]
        [Produces("application/json")]
        public async Task<ImmutableList<DailyStatistic>> GetAttendanceHistory([FromBody] IImmutableList<int> selectedLocations)
        {
            return await _overviewService.GetAttendanceHistory(selectedLocations);
        }
    }
}