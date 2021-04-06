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
    [Route(template: "[controller]")]
    public class CheckInOutController : ControllerBase
    {
        private readonly ICheckInOutService _checkInOutService;
        private readonly IUpdateTask _updateTask;

        public CheckInOutController(ICheckInOutService checkInOutService, IUpdateTask updateTask)
        {
            _checkInOutService = checkInOutService;
            _updateTask = updateTask;
        }

        [HttpPost]
        [Route(template: "manual")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> ManualCheckIn([FromBody] CheckInOutRequest request)
        {
            var checkInIds = request.CheckInOutCandidates.Select(selector: c => c.CheckInId).ToImmutableList();
            var success = request.CheckType switch
            {
                CheckType.CheckIn => await _checkInOutService.CheckInPeople(checkInIds: checkInIds).ConfigureAwait(continueOnCapturedContext: false),
                CheckType.CheckOut => await _checkInOutService.CheckOutPeople(checkInIds: checkInIds).ConfigureAwait(continueOnCapturedContext: false),
                _ => throw new ArgumentException(message: $"CheckType unknown: {request.CheckType}", paramName: nameof(request))
            };
            
            var names = request.CheckInOutCandidates.Select(selector: c => c.Name).ToImmutableList();

            if (success)
            {
                return Ok(value: new CheckInOutResult
                {
                    Text = $"{request.CheckType.ToString()} für {string.Join(separator: ", ", values: names)} war erfolgreich.",
                    AlertLevel = AlertLevel.Success,
                    AttendanceIds = checkInIds
                });
            }

            return Ok(value: new CheckInOutResult
            {
                Text = $"{request.CheckType.ToString()} für {string.Join(separator: ", ", values: names)} ist fehlgeschlagen",
                AlertLevel = AlertLevel.Danger
            });
        }
        

        [HttpPost]
        [Route(template: "people")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> GetPeople([FromBody] CheckInOutRequest request)
        {
            _updateTask.ActivateTask();
            
            var people = await _checkInOutService.SearchForPeople(
                searchParameters: new PeopleSearchParameters(
                    securityCode: request.SecurityCode, 
                    eventId: request.EventId,
                    locationGroups: request.SelectedLocationIds)).ConfigureAwait(continueOnCapturedContext: false);

            if (people.Count == 0)
            {
                return Ok(value: new CheckInOutResult
                {
                    Text = $"Es wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations und CheckIn/CheckOut Einstellungen überprüfen.",
                    AlertLevel = AlertLevel.Danger
                });
            }
            
            var peopleReadyForProcessing = GetPeopleInRequestedState(people: people, checkType: request.CheckType);
            
            if (peopleReadyForProcessing.Count == 0)
            {
                return Ok(value: new CheckInOutResult
                {
                    Text = $"Für {request.CheckType.ToString()} wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations und CheckIn/CheckOut Einstellungen überprüfen.",
                    AlertLevel = AlertLevel.Danger
                });
            }
            
            if (request.IsFastCheckInOut 
                && (!peopleReadyForProcessing.Any(predicate: p => !p.MayLeaveAlone || p.HasPeopleWithoutPickupPermission) 
                    || request.CheckType == CheckType.CheckIn))
            {
                var person = await TryFastCheckInOut(people: peopleReadyForProcessing, checkType: request.CheckType).ConfigureAwait(continueOnCapturedContext: false);
                if (person != null)
                {
                    return Ok(value: new CheckInOutResult
                    {
                        Text = $"{request.CheckType.ToString()} für {person.FirstName} {person.LastName} war erfolgreich.",
                        AlertLevel = AlertLevel.Success,
                        SuccessfulFastCheckout = true,
                        AttendanceIds = ImmutableList.Create(item: person.CheckInId)
                    });
                }
            }

            var checkInOutCandidates = peopleReadyForProcessing.Select(selector: p => new CheckInOutCandidate
            {
                CheckInId = p.CheckInId,
                Name = $"{p.FirstName} {p.LastName}",
                MayLeaveAlone = p.MayLeaveAlone,
                HasPeopleWithoutPickupPermission = p.HasPeopleWithoutPickupPermission
            }).ToImmutableList();

            var text = GetCandidateAlert(request: request, checkInOutCandidates: checkInOutCandidates, level: out var level);

            return Ok(value: new CheckInOutResult
            {
                Text = text,
                AlertLevel = level,
                CheckInOutCandidates = checkInOutCandidates
            });
        }
        
        [HttpPost]
        [Route(template: "undo/{checkType}")]
        [Produces(contentType: "application/json")]
        public async Task<IActionResult> Undo([FromRoute] CheckType checkType, [FromBody] ImmutableList<int> checkinIds)
        {
            var checkState = checkType switch
            {
                CheckType.CheckIn => CheckState.PreCheckedIn,
                CheckType.CheckOut => CheckState.CheckedIn,
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null)
            };
               
            var success = await _checkInOutService.UndoAction(revertedCheckState: checkState, checkinIds: checkinIds);

            if (success)
            {
                return Ok(value: new CheckInOutResult
                {
                    Text = $"Der {checkType} wurde erfolgreich rückgänig gemacht.",
                    AlertLevel = AlertLevel.Info,
                });    
            }
            
            return Ok(value: new CheckInOutResult
            {
                Text = "Rückgänig machen ist fehlgeschlagen",
                AlertLevel = AlertLevel.Danger,
            }); 
            
        }

        private static string GetCandidateAlert(CheckInOutRequest request, ImmutableList<CheckInOutCandidate> checkInOutCandidates,
            out AlertLevel level)
        {
            var text = "";
            level = AlertLevel.Info;

            if (request.CheckType != CheckType.CheckOut)
            {
                return text;
            }
            
            if (checkInOutCandidates.Any(predicate: c => !c.MayLeaveAlone))
            {
                text = "Kinder mit gelbem Hintergrund dürfen nicht alleine gehen";
                level = AlertLevel.Warning;
            }

            if (!checkInOutCandidates.Any(predicate: c => c.HasPeopleWithoutPickupPermission))
            {
                return text;
            }
            
            text = "Bei Kindern mit rotem Hintergrund gibt es Personen, die nicht abholberechtigt sind";
            level = AlertLevel.Danger;

            return text;
        }

        private async Task<Person?> TryFastCheckInOut(IImmutableList<Person> people, CheckType checkType)
        {
            switch (checkType)
            {
                case CheckType.CheckIn:
                    if (people.Count == 1)
                    {
                        var success = await _checkInOutService.CheckInPeople(checkInIds: people.Select(selector: p => p.CheckInId).ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);
                        return success ? people.Single() : null;
                    }
                    break;
                case CheckType.CheckOut:
                    if (people.Count == 1)
                    {
                        var success = await _checkInOutService.CheckOutPeople(checkInIds: people.Select(selector: p => p.CheckInId).ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);
                        return success ? people.Single() : null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null);
            }

            return null;
        }

        private static ImmutableList<Person> GetPeopleInRequestedState(IImmutableList<Person> people, CheckType checkType)
        {
            return checkType switch
            {
                CheckType.CheckIn => people.Where(predicate: p => p.CheckState == CheckState.PreCheckedIn).ToImmutableList(),
                CheckType.CheckOut => people.Where(predicate: p => p.CheckState == CheckState.CheckedIn).ToImmutableList(),
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null)
            };
        }
    }
}