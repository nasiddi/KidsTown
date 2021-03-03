using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Application.Models;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using CheckInsExtension.CheckInUpdateJobs.Update;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckInOutController : ControllerBase
    {
        private readonly ICheckInOutService _checkInOutService;
        private readonly UpdateTask _updateTask;

        public CheckInOutController(ICheckInOutService checkInOutService, UpdateTask updateTask)
        {
            _checkInOutService = checkInOutService;
            _updateTask = updateTask;
        }

        [HttpGet]
        [Route("location")]
        [Produces("application/json")]
        public async Task<ImmutableList<Options>> GetLocations()
        {
            var locations = await _checkInOutService.GetActiveLocations();

            return locations.Select(MapOptions).ToImmutableList();
        }
        
        [HttpPost]
        [Route("attendees/active")]
        [Produces("application/json")]
        public async Task<ImmutableList<Attendee>> GetActiveAttendees([FromBody] IImmutableList<int> selectedLocations)
        {
            return await _checkInOutService.GetActiveAttendees(selectedLocations);
        }
        
        
        
        [HttpPost]
        [Route("manual")]
        [Produces("application/json")]
        public async Task<IActionResult> ManualCheckIn([FromBody] CheckInOutRequest request)
        {
            var checkInIds = request.CheckInOutCandidates.Select(c => c.CheckInId).ToImmutableList();
            var success = request.CheckType switch
            {
                CheckType.CheckIn => await _checkInOutService.CheckInPeople(checkInIds),
                CheckType.CheckOut => await _checkInOutService.CheckOutPeople(checkInIds),
                _ => throw new ArgumentOutOfRangeException(nameof(request.CheckType), request.CheckType, null)
            };
            
            var names = request.CheckInOutCandidates.Select(c => c.Name).ToImmutableList();

            if (success)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"{request.CheckType.ToString()} für {string.Join(", ", names)} war erfolgreich.",
                    AlertLevel = AlertLevel.Success,
                });
            }

            return Ok(new CheckInOutResult
            {
                Text = $"{request.CheckType.ToString()} für {string.Join(", ", names)} ist fehlgeschlagen",
                AlertLevel = AlertLevel.Error,
            });
        }
        

        [HttpPost]
        [Route("people")]
        [Produces("application/json")]
        public async Task<IActionResult> GetPeople([FromBody] CheckInOutRequest request)
        {
            _updateTask.TaskIsActive = true;
            
            var people = await _checkInOutService.SearchForPeople(
                new PeopleSearchParameters(
                    request.SecurityCode, 
                    request.SelectedLocationIds));

            if (people.Count == 0)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Es wurde niemand mit SecurityCode {request.SecurityCode} gefunden.",
                    AlertLevel = AlertLevel.Error
                });
            }
            
            var peopleReadyForProcessing = GetPeopleInRequestedState(people, request.CheckType);
            
            if (peopleReadyForProcessing.Count == 0)
            {
                return Ok(new CheckInOutResult
                {
                    Text = $"Für {request.CheckType.ToString()} wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations und CheckIn/CheckOut überprüfen",
                    AlertLevel = AlertLevel.Error
                });
            }
            
            if (request.IsFastCheckInOut)
            {
                var person = await TryFastCheckInOut(peopleReadyForProcessing, request.CheckType);
                if (person != null)
                {
                    return Ok(new CheckInOutResult
                    {
                        Text = $"{request.CheckType.ToString()} für {person.FirstName} {person.LastName} war erfolgreich.",
                        AlertLevel = AlertLevel.Success,
                        SuccessfulFastCheckout = true,
                    });
                }
            }

            var checkInOutCandidates = peopleReadyForProcessing.Select(p => new CheckInOutCandidate
            {
                CheckInId = p.CheckInId,
                Name = $"{p.FirstName} {p.LastName}",
                MayLeaveAlone = p.MayLeaveAlone,
                HasPeopleWithoutPickupPermission = p.HasPeopleWithoutPickupPermission
            }).ToImmutableList();

            var text = "";
            var level = AlertLevel.Info;

            if (checkInOutCandidates.Any(c => !c.MayLeaveAlone))
            {
                text = "Kinder mit gelbem Hintergrund dürfen nicht alleine gehen";
                level = AlertLevel.Warning;
            }

            if (checkInOutCandidates.Any(c => c.HasPeopleWithoutPickupPermission))
            {
                text = "Bei Kindern mit rotem Hintergrund gibt es Personen, die nicht abholberechtigt sind";
                level = AlertLevel.Error;
            }

            return Ok(new CheckInOutResult
            {
                Text = text,
                AlertLevel = level,
                CheckInOutCandidates = checkInOutCandidates
            });
        }

        private async Task<Person?> TryFastCheckInOut(IImmutableList<Person> people, CheckType checkType)
        {
            switch (checkType)
            {
                case CheckType.CheckIn:
                    if (people.Count == 1)
                    {
                        var success = await _checkInOutService.CheckInPeople(people.Select(p => p.CheckInId).ToImmutableList());
                        return success ? people.Single() : null;
                    }
                    break;
                case CheckType.CheckOut:
                    if (people.Count == 1)
                    {
                        var success = await _checkInOutService.CheckOutPeople(people.Select(p => p.CheckInId).ToImmutableList());
                        return success ? people.Single() : null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(checkType), checkType, null);
            }

            return null;
        }

        private static ImmutableList<Person> GetPeopleInRequestedState(IImmutableList<Person> people, CheckType checkType)
        {
            return checkType switch
            {
                CheckType.CheckIn => people.Where(p => p.CheckState == CheckState.PreCheckedIn).ToImmutableList(),
                CheckType.CheckOut => people.Where(p => p.CheckState == CheckState.CheckedIn).ToImmutableList(),
                _ => throw new ArgumentOutOfRangeException(nameof(checkType), checkType, null)
            };
        }

        private static Options MapOptions(Location location)
        {
            return new()
            {
                Value = location.Id,
                Label = location.Name
            };

        }
    }
}