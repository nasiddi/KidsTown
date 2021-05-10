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
            var checkIns = await _planningCenterClient.GetCheckedInPeople(daysLookBack)
                .ConfigureAwait(false);
            var locationUpdateCount = await UpdateLocations(checkIns).ConfigureAwait(false);
            
            var checkInsUpdates = await MapCheckInsUpdates(checkIns)
                .ConfigureAwait(false);
            var insertCount = await InsertNewPreCheckIns(checkInsUpdates).ConfigureAwait(false);

            var volunteerCheckCount = await _attendanceUpdateRepository.AutoCheckInVolunteers().ConfigureAwait(false);

            return locationUpdateCount + insertCount + volunteerCheckCount;
        }
        
        private async Task<int> InsertNewPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns)
        {
            var existingChecksInIds = await _attendanceUpdateRepository.GetPersistedCheckInsIds(
                    preCheckIns.Select(i => i.CheckInsId).ToImmutableList())
                .ConfigureAwait(false);

            var newCheckins = preCheckIns.Where(p => !existingChecksInIds.Contains(p.CheckInsId))
                .ToImmutableList();

            return await _attendanceUpdateRepository.InsertAttendances(newCheckins)
                .ConfigureAwait(false);
        }
        
        private async Task<IImmutableList<CheckInsUpdate>> MapCheckInsUpdates(IImmutableList<CheckIns> checkIns)
        {
            var persistedLocations = await _attendanceUpdateRepository.GetPersistedLocations()
                .ConfigureAwait(false);
            var locationIdsByCheckInsLocationId =
                persistedLocations.ToImmutableDictionary(keySelector: k => k.CheckInsLocationId,
                    elementSelector: v => v.LocationId);

            var checkInsUpdates = checkIns.Where(c => c.Attendees != null)
                .SelectMany(c => c.Attendees!)
                .Select(a => MapCheckInsUpdate(
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
                             locationIdsByCheckInsLocationId.ContainsKey(checkInsLocationId.Value)
                ? locationIdsByCheckInsLocationId[checkInsLocationId.Value]
                : 30;

            return new CheckInsUpdate(
                checkInsId: attendee.Id,
                peopleId: peopleId,
                attendeeType: attributes?.Kind ?? AttendeeType.Regular,
                securityCode: attributes?.SecurityCode ?? string.Empty,
                locationId: locationId,
                creationDate: attributes?.CreatedAt ?? DateTime.UtcNow,
                kid: peopleUpdate);
        }

        private async Task<int> UpdateLocations(IImmutableList<CheckIns> checkIns)
        {
            var locations = checkIns.SelectMany(c
                => c.Included?.Where(i => i.Type == IncludeType.Location).ToImmutableList() ??
                   ImmutableList<Included>.Empty);


            var persistedLocations = await _attendanceUpdateRepository.GetPersistedLocations()
                .ConfigureAwait(false);
            var newLocations = locations
                .Where(l => IsNewLocation(persistedLocations: persistedLocations, location: l))
                .ToImmutableList();

            if (newLocations.Count == 0)
            {
                return 0;
            }

            var locationUpdateCount = await _attendanceUpdateRepository.UpdateLocations(newLocations.Select(l
                    => MapLocationUpdate(
                        location: l,
                        attendees: checkIns.Where(c => c.Attendees != null)
                            .SelectMany(c => c.Attendees!).ToImmutableList()))
                .ToImmutableList()).ConfigureAwait(false);


            await _attendanceUpdateRepository.EnableUnknownLocationGroup().ConfigureAwait(false);

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
            var attendee = attendees.FirstOrDefault(a
                => a.Relationships?.Locations?.Data?.SingleOrDefault()?.Id == location.Id);
            return new LocationUpdate(
                checkInsLocationId: location.Id,
                name: location.Attributes?.Name ?? string.Empty,
                eventId: attendee?.Relationships?.Event?.Data?.Id ?? 0);
        }
    }
}