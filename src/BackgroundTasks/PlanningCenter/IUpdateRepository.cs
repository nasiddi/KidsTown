using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Models;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public interface IUpdateRepository
    {
        Task InsertPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns);
        Task<IImmutableList<long>> GetExistingCheckInsIds(IImmutableList<long> checkinsIds);
        Task<ImmutableList<long>> GetCurrentPeopleIds(int daysLookBack);
        Task UpdateKids(ImmutableList<PeopleUpdate> kids, ImmutableList<Family> immutableList);
        Task AutoCheckInVolunteers();
        Task AutoCheckoutEveryoneByEndOfDay();
        Task<ImmutableList<PersistedLocation>> GetPersistedLocations();
        Task UpdateLocations(ImmutableList<LocationUpdate> locationUpdates);
        Task EnableUnknownLocationGroup();
        Task LogTaskRun(bool success, int updateCount, string environment);
        Task<ImmutableList<Family>> GetExistingFamilies(ImmutableList<long> householdIds);
        Task<ImmutableList<Family>> InsertFamilies(ImmutableList<long> newHouseholdIds, ImmutableList<PeopleUpdate> peoples);
        Task UpdateParents(IImmutableList<ParentUpdate> parentUpdates);
    }
}