using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

namespace KidsTown.BackgroundTasks.Attendance
{
    public class AttendanceUpdateService : IAttendanceUpdateService
    {
        private readonly IPlanningCenterClient _planningCenterClient;
        private readonly IAttendanceUpdateRepository _attendanceUpdateRepository;

        public AttendanceUpdateService(IPlanningCenterClient planningCenterClient, 
            IAttendanceUpdateRepository attendanceUpdateRepository)
        {
            _planningCenterClient = planningCenterClient;
            _attendanceUpdateRepository = attendanceUpdateRepository;
        }
        
        public async Task<int> UpdateAttendance(int daysLookBack)
        {
            var checkIns = await _planningCenterClient.GetCheckedInPeople(daysLookBack: daysLookBack)
                .ConfigureAwait(continueOnCapturedContext: false);
            var locationUpdateCount = await UpdateLocations(checkIns: checkIns).ConfigureAwait(continueOnCapturedContext: false);
            
            var checkInsUpdates = await MapCheckInsUpdates(checkIns: checkIns)
                .ConfigureAwait(continueOnCapturedContext: false);
            var insertCount = await InsertNewPreCheckIns(preCheckIns: checkInsUpdates).ConfigureAwait(continueOnCapturedContext: false);

            var volunteerCheckCount = await _attendanceUpdateRepository.AutoCheckInVolunteers().ConfigureAwait(continueOnCapturedContext: false);

            return locationUpdateCount + insertCount + volunteerCheckCount;
        }
        
        private async Task<int> InsertNewPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns)
        {
            var existingChecksInIds = await _attendanceUpdateRepository.GetPersistedCheckInsIds(
                    checkinsIds: preCheckIns.Select(selector: i => i.CheckInsId).ToImmutableList())
                .ConfigureAwait(continueOnCapturedContext: false);

            var newCheckins = preCheckIns.Where(predicate: p => !existingChecksInIds.Contains(value: p.CheckInsId))
                .ToImmutableList();

            return await _attendanceUpdateRepository.InsertAttendances(checkInsUpdates: newCheckins)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private async Task<IImmutableList<CheckInsUpdate>> MapCheckInsUpdates(IImmutableList<CheckIns> checkIns)
        {
            var persistedLocations = await _attendanceUpdateRepository.GetPersistedLocations()
                .ConfigureAwait(continueOnCapturedContext: false);
            var locationIdsByCheckInsLocationId =
                persistedLocations.ToImmutableDictionary(keySelector: k => k.CheckInsLocationId,
                    elementSelector: v => v.LocationId);

            var checkInsUpdates = checkIns.Where(predicate: c => c.Attendees != null)
                .SelectMany(selector: c => c.Attendees!)
                .Select(selector: a => MapCheckInsUpdate(
                    attendee: a,
                    locationIdsByCheckInsLocationId: locationIdsByCheckInsLocationId))
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

            var peopleUpdate = new PeopleUpdate(
                peopleId: peopleId,
                firstName: attributes?.FirstName ?? string.Empty,
                lastName: attributes?.LastName ?? string.Empty);

            var locationId = checkInsLocationId.HasValue &&
                             locationIdsByCheckInsLocationId.ContainsKey(key: checkInsLocationId.Value)
                ? locationIdsByCheckInsLocationId[key: checkInsLocationId.Value]
                : 30;

            return new CheckInsUpdate(
                checkInsId: attendee.Id,
                peopleId: peopleId,
                attendeeType: attributes?.Kind ?? AttendeeType.Regular,
                securityCode: attributes?.SecurityCode ?? string.Empty,
                locationId: locationId,
                creationDate: attributes?.CreatedAt ?? DateTime.UtcNow,
                kid: peopleUpdate,
                emergencyContactName: attributes?.EmergencyContactName,
                emergencyContactNumber: attributes?.EmergencyContactPhoneNumber);
        }

        private async Task<int> UpdateLocations(IImmutableList<CheckIns> checkIns)
        {
            var locations = checkIns.SelectMany(selector: c
                => c.Included?.Where(predicate: i => i.Type == IncludeType.Location).ToImmutableList() ??
                   ImmutableList<Included>.Empty);


            var persistedLocations = await _attendanceUpdateRepository.GetPersistedLocations()
                .ConfigureAwait(continueOnCapturedContext: false);
            var newLocations = locations
                .Where(predicate: l => IsNewLocation(persistedLocations: persistedLocations, location: l))
                .ToImmutableList();

            if (newLocations.Count == 0)
            {
                return 0;
            }

            var locationUpdateCount = await _attendanceUpdateRepository.UpdateLocations(locationUpdates: newLocations.Select(selector: l
                    => MapLocationUpdate(
                        location: l,
                        attendees: checkIns.Where(predicate: c => c.Attendees != null)
                            .SelectMany(selector: c => c.Attendees!).ToImmutableList()))
                .ToImmutableList()).ConfigureAwait(continueOnCapturedContext: false);


            await _attendanceUpdateRepository.EnableUnknownLocationGroup().ConfigureAwait(continueOnCapturedContext: false);

            return locationUpdateCount;
        }

        private static bool IsNewLocation(
            IImmutableList<PersistedLocation> persistedLocations, 
            Included location
        )
        {
            return !persistedLocations.Select(selector: p => p.CheckInsLocationId).Contains(value: location.Id);
        }
        
        private static LocationUpdate MapLocationUpdate(
            Included location,
            IImmutableList<Attendee> attendees
        )
        {
            var attendee = attendees.FirstOrDefault(predicate: a
                => a.Relationships?.Locations?.Data?.SingleOrDefault()?.Id == location.Id);
            return new LocationUpdate(
                checkInsLocationId: location.Id,
                name: location.Attributes?.Name ?? string.Empty,
                eventId: attendee?.Relationships?.Event?.Data?.Id ?? 0);
        }
    }
}