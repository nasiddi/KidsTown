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
using Org.BouncyCastle.Crypto.Digests;

namespace KidsTown.Application.Controllers;

[ApiController]
[AuthenticateUser]
[Route("[controller]")]
public class CheckInOutController(
        ICheckInOutService checkInOutService,
        ITaskManagementService taskManagementService,
        ISearchLoggingService searchLoggingService)
    : ControllerBase
{
    [HttpPost]
    [Route("manual-with-location-update")]
    [Produces("application/json")]
    public async Task<IActionResult> ManualLocationUpdateAndCheckIn([FromBody] CheckInOutCandidate candidate)
    {
        var success = await checkInOutService.UpdateLocationAndCheckIn(
            candidate.AttendanceId,
            candidate.LocationId);

        if (success)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text = $"{CheckType.CheckIn.ToString()} für {candidate.Name} war erfolgreich.",
                    AlertLevel = AlertLevel.Success,
                    AttendanceIds = ImmutableList.Create(candidate.AttendanceId)
                });
        }

        return Ok(
            new CheckInOutResult
            {
                Text = $"{CheckType.CheckIn.ToString()} für {candidate.Name} ist fehlgeschlagen",
                AlertLevel = AlertLevel.Error
            });
    }

    [HttpPost]
    [Route("manual")]
    [Produces("application/json")]
    public async Task<IActionResult> ManualCheckIn([FromBody] CheckInOutRequest request)
    {
        var attendanceIds = request.CheckInOutCandidates.Select(c => c.AttendanceId).ToImmutableList();
        var success =
            await checkInOutService.CheckInOutPeople(request.CheckType, attendanceIds);

        var names = request.CheckInOutCandidates.Select(c => c.Name).ToImmutableList();

        if (success)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text =
                        $"{request.CheckType.ToString()} für ({request.SecurityCode}) {string.Join(", ", names)} war erfolgreich.",
                    AlertLevel = AlertLevel.Success,
                    AttendanceIds = attendanceIds
                });
        }

        return Ok(
            new CheckInOutResult
            {
                Text =
                    $"{request.CheckType.ToString()} für ({request.SecurityCode}) {string.Join(", ", names)} ist fehlgeschlagen",
                AlertLevel = AlertLevel.Error
            });
    }

    [HttpPost]
    [Route("people")]
    [Produces("application/json")]
    public async Task<IActionResult> GetPeople([FromBody] CheckInOutRequest request)
    {
        taskManagementService.ActivateBackgroundTasks();

        switch (request.FilterLocations)
        {
            case false when request.CheckType != CheckType.CheckIn:
                return Ok(
                    new CheckInOutResult
                    {
                        Text = "Suche ohne Location Filter ist nur bei CheckIn erlaubt.",
                        AlertLevel = AlertLevel.Error
                    });
            case true when request.SelectedLocationGroupIds.Count == 0:
                return Ok(
                    new CheckInOutResult
                    {
                        Text = "Bitte Locations auswählen.",
                        AlertLevel = AlertLevel.Error
                    });
        }

        if (request.SecurityCode.StartsWith(value: '1') && request.CheckType == CheckType.CheckIn)
        {
            await checkInOutService.CreateUnregisteredGuest(
                request.SecurityCode,
                request.EventId,
                request.SelectedLocationGroupIds);
        }

        var peopleSearchParameters = new PeopleSearchParameters(
            request.SecurityCode,
            request.EventId,
            request.SelectedLocationGroupIds,
            request.FilterLocations);

        var people = await checkInOutService.SearchForPeople(peopleSearchParameters);

        await searchLoggingService.LogSearch(
            peopleSearchParameters,
            people,
            request.Guid,
            request.CheckType,
            request.FilterLocations);

        if (people.Count == 0)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text =
                        $"Es wurde niemand mit SecurityCode {request.SecurityCode} gefunden. Locations Filter überprüfen.",
                    AlertLevel = AlertLevel.Error,
                    FilteredSearchUnsuccessful = true
                });
        }

        var peopleReadyForProcessing = GetPeopleInRequestedState(people, request.CheckType);

        if (peopleReadyForProcessing.Count == 0)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text =
                        $"Niemand mit {request.SecurityCode} ist bereit für {request.CheckType.ToString()}. CheckIn/Out Einstellungen überprüfen.",
                    AlertLevel = AlertLevel.Warning,
                    FilteredSearchUnsuccessful = true
                });
        }

        if (request.IsFastCheckInOut
            && request.FilterLocations
            && (!peopleReadyForProcessing.Any(p => !p.MayLeaveAlone || p.HasPeopleWithoutPickupPermission)
                || request.CheckType == CheckType.CheckIn))
        {
            var kid = await TryFastCheckInOut(peopleReadyForProcessing, request.CheckType);

            if (kid != null)
            {
                return Ok(
                    new CheckInOutResult
                    {
                        Text =
                            $"{request.CheckType.ToString()} für ({request.SecurityCode}) {kid.FirstName} {kid.LastName} war erfolgreich.",
                        AlertLevel = AlertLevel.Success,
                        SuccessfulFastCheckout = true,
                        AttendanceIds = ImmutableList.Create(kid.AttendanceId)
                    });
            }
        }

        var checkInOutCandidates = peopleReadyForProcessing.Select(
                p =>
                    new CheckInOutCandidate(
                        p.AttendanceId,
                        $"{p.FirstName} {p.LastName}",
                        p.LocationGroupId,
                        p.MayLeaveAlone,
                        p.HasPeopleWithoutPickupPermission))
            .ToImmutableList();

        var text = GetCandidateAlert(
            request,
            checkInOutCandidates,
            out var level);

        return Ok(
            new CheckInOutResult
            {
                Text = text,
                AlertLevel = level,
                CheckInOutCandidates = checkInOutCandidates
            });
    }

    [HttpPost]
    [Route("undo/{checkType}")]
    [Produces("application/json")]
    public async Task<IActionResult> Undo([FromRoute] CheckType checkType, [FromBody] IImmutableList<int> attendanceIds)
    {
        var checkState = checkType switch
        {
            CheckType.GuestCheckIn => CheckState.None,
            CheckType.CheckIn => CheckState.PreCheckedIn,
            CheckType.CheckOut => CheckState.CheckedIn,
            _ => throw new ArgumentOutOfRangeException(
                nameof(checkType),
                checkType,
                message: null)
        };

        var success = await checkInOutService.UndoAction(checkState, attendanceIds);

        if (success)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text = $"Der {checkType} wurde erfolgreich rückgänig gemacht.",
                    AlertLevel = AlertLevel.Info
                });
        }

        return Ok(
            new CheckInOutResult
            {
                Text = "Rückgänig machen ist fehlgeschlagen",
                AlertLevel = AlertLevel.Error
            });
    }

    [HttpPost]
    [Route("guest/checkin")]
    [Produces("application/json")]
    public async Task<IActionResult> CheckInGuest([FromBody] GuestCheckInRequest request)
    {
        var attendanceId = await checkInOutService.CreateGuest(
            request.LocationGroupId,
            request.SecurityCode,
            request.FirstName,
            request.LastName);

        if (attendanceId == null)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text = $"Der SecurityCode {request.SecurityCode} existiert bereits.",
                    AlertLevel = AlertLevel.Error
                });
        }

        await checkInOutService.CheckInOutPeople(
            CheckType.CheckIn,
            ImmutableList.Create(attendanceId.Value));

        return Ok(
            new CheckInOutResult
            {
                Text = $"CheckIn für {request.FirstName} {request.LastName} war erfolgreich.",
                AlertLevel = AlertLevel.Success,
                AttendanceIds = ImmutableList.Create(attendanceId.Value)
            });
    }

    [HttpPost]
    [Route("guest/create")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateGuest([FromBody] GuestCheckInRequest request)
    {
        var attendanceId = await checkInOutService.CreateGuest(
            request.LocationGroupId,
            request.SecurityCode,
            request.FirstName,
            request.LastName);

        if (attendanceId == null)
        {
            return Ok(
                new CheckInOutResult
                {
                    Text = $"Der SecurityCode {request.SecurityCode} existiert bereits.",
                    AlertLevel = AlertLevel.Error
                });
        }

        return Ok(
            new CheckInOutResult
            {
                Text = $"Erfassen von {request.FirstName} {request.LastName} war erfolgreich.",
                AlertLevel = AlertLevel.Success,
                AttendanceIds = ImmutableList.Create(attendanceId.Value)
            });
    }

    private static string GetCandidateAlert(
        CheckInOutRequest request,
        IImmutableList<CheckInOutCandidate> checkInOutCandidates,
        out AlertLevel level)
    {
        var text = "";
        level = AlertLevel.Info;

        if (request.CheckType != CheckType.CheckOut)
        {
            return text;
        }

        if (checkInOutCandidates.Any(c => !c.MayLeaveAlone))
        {
            text = "Kinder mit gelbem Hintergrund dürfen nicht alleine gehen";
            level = AlertLevel.Warning;
        }

        if (!checkInOutCandidates.Any(c => c.HasPeopleWithoutPickupPermission))
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

        var success = await checkInOutService
                .CheckInOutPeople(
                    checkType,
                    people.Select(p => p.AttendanceId).ToImmutableList());

        return success ? people.Single() : null;
    }

    private static IImmutableList<Kid> GetPeopleInRequestedState(IImmutableList<Kid> people, CheckType checkType)
    {
        return checkType switch
        {
            CheckType.CheckIn => people.Where(p => p.CheckState == CheckState.PreCheckedIn)
                .ToImmutableList(),
            CheckType.CheckOut => people.Where(p => p.CheckState == CheckState.CheckedIn).ToImmutableList(),
            CheckType.GuestCheckIn => throw new ArgumentException(
                $"Unexpected CheckType: {checkType}",
                nameof(checkType)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(checkType),
                checkType,
                message: null)
        };
    }
}