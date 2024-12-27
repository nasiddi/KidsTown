using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

namespace KidsTown.BackgroundTasks.Attendance;

public class AttendanceUpdateService(
        IPlanningCenterClient planningCenterClient,
        IAttendanceUpdateRepository attendanceUpdateRepository)
    : IAttendanceUpdateService
{
    public async Task<int> UpdateAttendance(int daysLookBack)
    {
        var checkIns = await planningCenterClient.GetCheckedInPeople(daysLookBack)
            .ConfigureAwait(continueOnCapturedContext: false);

        var locationUpdateCount = await UpdateLocations(checkIns).ConfigureAwait(continueOnCapturedContext: false);

        var checkInsUpdates = await MapCheckInsUpdates(checkIns)
            .ConfigureAwait(continueOnCapturedContext: false);

        var insertCount = await InsertNewPreCheckIns(checkInsUpdates).ConfigureAwait(continueOnCapturedContext: false);

        var volunteerCheckCount = await attendanceUpdateRepository.AutoCheckInVolunteers().ConfigureAwait(continueOnCapturedContext: false);

        return locationUpdateCount + insertCount + volunteerCheckCount;
    }

    private async Task<int> InsertNewPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns)
    {
        var existingChecksInIds = await attendanceUpdateRepository.GetPersistedCheckInsIds(preCheckIns.Select(i => i.CheckInsId).ToImmutableList())
            .ConfigureAwait(continueOnCapturedContext: false);

        var newCheckins = preCheckIns.Where(p => !existingChecksInIds.Contains(p.CheckInsId))
            .ToImmutableList();

        return await attendanceUpdateRepository.InsertAttendances(newCheckins)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<IImmutableList<CheckInsUpdate>> MapCheckInsUpdates(IImmutableList<CheckIns> checkIns)
    {
        var persistedLocations = await attendanceUpdateRepository.GetPersistedLocations()
            .ConfigureAwait(continueOnCapturedContext: false);

        var locationIdsByCheckInsLocationId =
            persistedLocations.ToImmutableDictionary(
                k => k.CheckInsLocationId,
                v => v.LocationId);

        var checkInsUpdates = checkIns.Where(c => c.Attendees != null)
            .SelectMany(c => c.Attendees!)
            .Select(
                a => MapCheckInsUpdate(
                    a,
                    locationIdsByCheckInsLocationId))
            .ToImmutableList();

        return checkInsUpdates;
    }

    private static CheckInsUpdate MapCheckInsUpdate(
        Attendee attendee,
        ImmutableDictionary<long, int> locationIdsByCheckInsLocationId
    )
    {
        var attributes = attendee.Attributes;
        var checkInsLocationId = attendee.Relationships?.Locations?.Data?.FirstOrDefault()?.Id;
        var peopleId = attendee.Relationships?.Person?.Data?.Id;

        var peopleUpdate = PeopleUpdate.CreateSimple(
            peopleId,
            attributes?.FirstName ?? string.Empty,
            attributes?.LastName ?? string.Empty);

        var locationId = checkInsLocationId.HasValue && locationIdsByCheckInsLocationId.ContainsKey(checkInsLocationId.Value)
            ? locationIdsByCheckInsLocationId[checkInsLocationId.Value]
            : 30;

        return new CheckInsUpdate(
            attendee.Id,
            peopleId,
            attributes?.Kind ?? AttendeeType.Regular,
            attributes?.SecurityCode ?? string.Empty,
            locationId,
            attributes?.CreatedAt ?? DateTime.UtcNow,
            peopleUpdate,
            attributes?.EmergencyContactName,
            attributes?.EmergencyContactPhoneNumber);
    }

    private async Task<int> UpdateLocations(IImmutableList<CheckIns> checkIns)
    {
        var locations = checkIns.SelectMany(
            c
                => c.Included?.Where(i => i.Type == IncludeType.Location).ToImmutableList() ?? ImmutableList<Included>.Empty);

        var persistedLocations = await attendanceUpdateRepository.GetPersistedLocations()
            .ConfigureAwait(continueOnCapturedContext: false);

        var newLocations = locations
            .Where(l => IsNewLocation(persistedLocations, l))
            .ToImmutableList();

        if (newLocations.Count == 0)
        {
            return 0;
        }

        var locationUpdateCount = await attendanceUpdateRepository.UpdateLocations(
                newLocations.Select(
                        l
                            => MapLocationUpdate(
                                l,
                                checkIns.Where(c => c.Attendees != null)
                                    .SelectMany(c => c.Attendees!)
                                    .ToImmutableList()))
                    .ToImmutableList())
            .ConfigureAwait(continueOnCapturedContext: false);

        await attendanceUpdateRepository.EnableUnknownLocationGroup().ConfigureAwait(continueOnCapturedContext: false);

        return locationUpdateCount;
    }

    private static bool IsNewLocation(
        IImmutableList<PersistedLocation> persistedLocations,
        Included location
    )
    {
        return !persistedLocations.Select(p => p.CheckInsLocationId).Contains(location.Id);
    }

    private static LocationUpdate MapLocationUpdate(
        Included location,
        IImmutableList<Attendee> attendees
    )
    {
        var attendee = attendees.FirstOrDefault(
            a
                => a.Relationships?.Locations?.Data?.SingleOrDefault()?.Id == location.Id);

        return new LocationUpdate(
            location.Id,
            location.Attributes?.Name ?? string.Empty,
            attendee?.Relationships?.Event?.Data?.Id ?? 0);
    }
}