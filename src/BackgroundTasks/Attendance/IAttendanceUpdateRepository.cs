using System.Collections.Immutable;
using System.Threading.Tasks;

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