using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.Common;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers;

[ApiController]
[Route(template: "[controller]")]
public class CheckInOutController : ControllerBase
{
    private readonly ICheckInOutService _checkInOutService;
    private readonly ITaskManagementService _taskManagementService;
    private readonly ISearchLoggingService _searchLoggingService;

    public CheckInOutController(ICheckInOutService checkInOutService, ITaskManagementService taskManagementService, ISearchLoggingService searchLoggingService)
    {
        _checkInOutService = checkInOutService;
        _taskManagementService = taskManagementService;
        _searchLoggingService = searchLoggingService;
    }

    [HttpPost]
    [Route(template: "manual-with-location-update")]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> ManualLocationUpdateAndCheckIn([FromBody] CheckInOutCandidate candidate)
    {
        var success = await _checkInOutService.UpdateLocationAndCheckIn(
            attendanceId: candidate.AttendanceId,
            locationId: candidate.LocationId);
            
        if (success)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"{CheckType.CheckIn.ToString()} für {candidate.Name} war erfolgreich.",
                AlertLevel = AlertLevel.Success,
                AttendanceIds = ImmutableList.Create(candidate.AttendanceId)
            });
        }

        return Ok(value: new CheckInOutResult
        {
            Text = $"{CheckType.CheckIn.ToString()} für {candidate.Name} ist fehlgeschlagen",
            AlertLevel = AlertLevel.Error
        });
    }

    [HttpPost]
    [Route(template: "manual")]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> ManualCheckIn([FromBody] CheckInOutRequest request)
    {
        var attendanceIds = request.CheckInOutCandidates.Select(selector: c => c.AttendanceId).ToImmutableList();
        var success = await _checkInOutService.CheckInOutPeople(checkType: request.CheckType, attendanceIds: attendanceIds);
            
        var names = request.CheckInOutCandidates.Select(selector: c => c.Name).ToImmutableList();

        if (success)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"{request.CheckType.ToString()} für ({request.SecurityCode}) {string.Join(separator: ", ", values: names)} war erfolgreich.",
                AlertLevel = AlertLevel.Success,
                AttendanceIds = attendanceIds
            });
        }

        return Ok(value: new CheckInOutResult
        {
            Text = $"{request.CheckType.ToString()} für ({request.SecurityCode}) {string.Join(separator: ", ", values: names)} ist fehlgeschlagen",
            AlertLevel = AlertLevel.Error
        });
    }

    [HttpPost]
    [Route(template: "people")]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> GetPeople([FromBody] CheckInOutRequest request)
    {
        _taskManagementService.ActivateBackgroundTasks();

        switch (request.FilterLocations)
        {
            case false when request.CheckType != CheckType.CheckIn:
                return Ok(value: new CheckInOutResult
                {
                    Text = "Suche ohne Location Filter ist nur bei CheckIn erlaubt.",
                    AlertLevel = AlertLevel.Error
                });
            case true when request.SelectedLocationGroupIds.Count == 0:
                return Ok(value: new CheckInOutResult
                {
                    Text = "Bitte Locations auswählen.",
                    AlertLevel = AlertLevel.Error
                });
        }

        if (request.SecurityCode.StartsWith(value: '1') && request.CheckType == CheckType.CheckIn)
        {
            await _checkInOutService.CreateUnregisteredGuest(
                requestSecurityCode: request.SecurityCode,
                requestEventId: request.EventId,
                requestSelectedLocationIds: request.SelectedLocationGroupIds);
        }

        var peopleSearchParameters = new PeopleSearchParameters(
            securityCode: request.SecurityCode, 
            eventId: request.EventId,
            locationGroups: request.SelectedLocationGroupIds,
            useFilterLocationGroups: request.FilterLocations);
            
        var people = await _checkInOutService.SearchForPeople(
            searchParameters: peopleSearchParameters).ConfigureAwait(continueOnCapturedContext: false);

        await _searchLoggingService.LogSearch(peopleSearchParameters: peopleSearchParameters,
            people: people,
            deviceGuid: request.Guid,
            checkType: request.CheckType,
            filterLocations: request.FilterLocations);
            
        if (people.Count == 0)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"Es wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations Filter überprüfen.",
                AlertLevel = AlertLevel.Error,
                FilteredSearchUnsuccessful = true
            });
        }
            
        var peopleReadyForProcessing = GetPeopleInRequestedState(people: people, checkType: request.CheckType);
            
        if (peopleReadyForProcessing.Count == 0)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"Niemand mit {request.SecurityCode} ist bereit für {request.CheckType.ToString()}. CheckIn/Out Einstellungen überprüfen.",
                AlertLevel = AlertLevel.Warning,
                FilteredSearchUnsuccessful = true
            });
        }
            
        if (request.IsFastCheckInOut 
            && request.FilterLocations
            && (!peopleReadyForProcessing.Any(predicate: p => !p.MayLeaveAlone || p.HasPeopleWithoutPickupPermission) 
                || request.CheckType == CheckType.CheckIn))
        {
            var kid = await TryFastCheckInOut(people: peopleReadyForProcessing, checkType: request.CheckType).ConfigureAwait(continueOnCapturedContext: false);
            if (kid != null)
            {
                return Ok(value: new CheckInOutResult
                {
                    Text = $"{request.CheckType.ToString()} für ({request.SecurityCode}) {kid.FirstName} {kid.LastName} war erfolgreich.",
                    AlertLevel = AlertLevel.Success,
                    SuccessfulFastCheckout = true,
                    AttendanceIds = ImmutableList.Create(item: kid.AttendanceId)
                });
            }
        }

        var checkInOutCandidates = peopleReadyForProcessing.Select(selector: p => new CheckInOutCandidate
        {
            AttendanceId = p.AttendanceId,
            Name = $"{p.FirstName} {p.LastName}",
            LocationId = p.LocationGroupId,
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
    public async Task<IActionResult> Undo([FromRoute] CheckType checkType, [FromBody] IImmutableList<int> attendanceIds)
    {
        var checkState = checkType switch
        {
            CheckType.GuestCheckIn => CheckState.None,
            CheckType.CheckIn => CheckState.PreCheckedIn,
            CheckType.CheckOut => CheckState.CheckedIn,
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null)
        };
               
        var success = await _checkInOutService.UndoAction(revertedCheckState: checkState, attendanceIds: attendanceIds);

        if (success)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"Der {checkType} wurde erfolgreich rückgänig gemacht.",
                AlertLevel = AlertLevel.Info
            });    
        }
            
        return Ok(value: new CheckInOutResult
        {
            Text = "Rückgänig machen ist fehlgeschlagen",
            AlertLevel = AlertLevel.Error
        }); 
            
    }

    [HttpPost]
    [Route(template: "guest/checkin")]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> CheckInGuest([FromBody] GuestCheckInRequest request)
    {
        var attendanceId = await _checkInOutService.CreateGuest(
            locationId: request.LocationGroupId,
            securityCode: request.SecurityCode,
            firstName: request.FirstName,
            lastName: request.LastName);

        if (attendanceId == null)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"Der SecurityCode {request.SecurityCode} existiert bereits.",
                AlertLevel = AlertLevel.Error
            });
        }

        await _checkInOutService.CheckInOutPeople(
            checkType: CheckType.CheckIn, 
            attendanceIds: ImmutableList.Create(item: attendanceId.Value));

        return Ok(value: new CheckInOutResult
        {
            Text = $"CheckIn für {request.FirstName} {request.LastName} war erfolgreich.",
            AlertLevel = AlertLevel.Success,
            AttendanceIds = ImmutableList.Create(item: attendanceId.Value)
        });
    }
        
    [HttpPost]
    [Route(template: "guest/create")]
    [Produces(contentType: "application/json")]
    public async Task<IActionResult> CreateGuest([FromBody] GuestCheckInRequest request)
    {
        var attendanceId = await _checkInOutService.CreateGuest(
            locationId: request.LocationGroupId,
            securityCode: request.SecurityCode,
            firstName: request.FirstName,
            lastName: request.LastName);

        if (attendanceId == null)
        {
            return Ok(value: new CheckInOutResult
            {
                Text = $"Der SecurityCode {request.SecurityCode} existiert bereits.",
                AlertLevel = AlertLevel.Error
            });
        }
            
        return Ok(value: new CheckInOutResult
        {
            Text = $"Erfassen von {request.FirstName} {request.LastName} war erfolgreich.",
            AlertLevel = AlertLevel.Success,
            AttendanceIds = ImmutableList.Create(item: attendanceId.Value)
        });

    }
        
    private static string GetCandidateAlert(CheckInOutRequest request, IImmutableList<CheckInOutCandidate> checkInOutCandidates,
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
        level = AlertLevel.Error;

        return text;
    }

    private async Task<Kid?> TryFastCheckInOut(IImmutableList<Kid> people, CheckType checkType)
    {
        if (people.Count != 1)
        {
            return null;
        }
            
        var success = await _checkInOutService
            .CheckInOutPeople(checkType: checkType, attendanceIds: people.Select(selector: p => p.AttendanceId).ToImmutableList())
            .ConfigureAwait(continueOnCapturedContext: false);
                
        return success ? people.Single() : null;

    }

    private static IImmutableList<Kid> GetPeopleInRequestedState(IImmutableList<Kid> people, CheckType checkType)
    {
        return checkType switch
        {
            CheckType.CheckIn => people.Where(predicate: p => p.CheckState == CheckState.PreCheckedIn).ToImmutableList(),
            CheckType.CheckOut => people.Where(predicate: p => p.CheckState == CheckState.CheckedIn).ToImmutableList(),
            CheckType.GuestCheckIn => throw new ArgumentException(message: $"Unexpected CheckType: {checkType}", paramName: nameof(checkType)),
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(checkType), actualValue: checkType, message: null)
        };
    }
}