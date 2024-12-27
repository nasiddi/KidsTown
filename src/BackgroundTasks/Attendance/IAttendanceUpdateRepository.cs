using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Common;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

namespace KidsTown.BackgroundTasks.Attendance;

public interface IAttendanceUpdateRepository
{
    Task<int> InsertAttendances(IImmutableList<CheckInsUpdate> checkInsUpdates);
    Task<IImmutableList<long>> GetPersistedCheckInsIds(IImmutableList<long> checkinsIds);
    Task<IImmutableList<PersistedLocation>> GetPersistedLocations();
    Task<int> UpdateLocations(IImmutableList<LocationUpdate> locationUpdates);
    Task EnableUnknownLocationGroup();
    Task<int> AutoCheckInVolunteers();
    Task<int> AutoCheckoutEveryoneByEndOfDay();
}

public record CheckInsUpdate(
    long CheckInsId,
    long? PeopleId,
    AttendeeType AttendeeType,
    string SecurityCode,
    int LocationId,
    DateTime CreationDate,
    PeopleUpdate Kid,
    string? EmergencyContactName,
    string? EmergencyContactNumber);

public record LocationUpdate(long CheckInsLocationId, string Name, long EventId);

public record PersistedLocation(int LocationId, long CheckInsLocationId);