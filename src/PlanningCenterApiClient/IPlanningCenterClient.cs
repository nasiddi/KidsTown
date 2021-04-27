using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using People = KidsTown.PlanningCenterApiClient.Models.PeopleResult.People;

namespace KidsTown.PlanningCenterApiClient
{
    public interface IPlanningCenterClient
    {
        Task<ImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack);
        Task<ImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds);
        Task<Household?> GetHousehold(long householdId);
        Task<Event> GetActiveEvents();
    }
}
