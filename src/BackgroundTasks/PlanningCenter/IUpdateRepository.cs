using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Common;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public interface IUpdateRepository
    {
        Task<int> InsertPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns);
        Task<IImmutableList<long>> GetExistingCheckInsIds(IImmutableList<long> checkinsIds);
        Task<ImmutableList<TypedAttendee>> GetCurrentPeopleIds(int daysLookBack);
        Task<int> UpdateKids(IImmutableList<PeopleUpdate> kids, IImmutableList<Family> families);
        Task<int> AutoCheckInVolunteers();
        Task<int> AutoCheckoutEveryoneByEndOfDay();
        Task<IImmutableList<PersistedLocation>> GetPersistedLocations();
        Task<int> UpdateLocations(IImmutableList<LocationUpdate> locationUpdates);
        Task EnableUnknownLocationGroup();
        Task LogTaskRun(bool success, int updateCount, string environment, string taskName);
        Task<IImmutableList<Family>> GetExistingFamilies(IImmutableList<long> householdIds);
        Task<IImmutableList<Family>> InsertFamilies(IImmutableList<long> newHouseholdIds, IImmutableList<PeopleUpdate> peoples);
        Task<int> UpdateParents(IImmutableList<AdultUpdate> parentUpdates);
    }
}