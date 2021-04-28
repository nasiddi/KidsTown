using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Models;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public interface IUpdateRepository
    {
        Task InsertPreCheckIns(IImmutableList<CheckInsUpdate> preCheckIns);
        Task<IImmutableList<long>> GetExistingCheckInsIds(IImmutableList<long> checkinsIds);
        Task<IImmutableList<long>> GetCurrentPeopleIds(int daysLookBack);
        Task UpdateKids(IImmutableList<PeopleUpdate> kids, IImmutableList<Family> families);
        Task AutoCheckInVolunteers();
        Task AutoCheckoutEveryoneByEndOfDay();
        Task<IImmutableList<PersistedLocation>> GetPersistedLocations();
        Task UpdateLocations(IImmutableList<LocationUpdate> locationUpdates);
        Task EnableUnknownLocationGroup();
        Task LogTaskRun(bool success, int updateCount, string environment);
        Task<IImmutableList<Family>> GetExistingFamilies(IImmutableList<long> householdIds);
        Task<IImmutableList<Family>> InsertFamilies(IImmutableList<long> newHouseholdIds, IImmutableList<PeopleUpdate> peoples);
        Task UpdateParents(IImmutableList<ParentUpdate> parentUpdates);
    }
}