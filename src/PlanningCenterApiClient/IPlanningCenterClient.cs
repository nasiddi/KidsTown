using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.PlanningCenterApiClient.Models.CheckInResult;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;

namespace KidsTown.PlanningCenterApiClient
{
    public interface IPlanningCenterClient
    {
        Task<ImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack);
        Task<ImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds);
        Task<Event> GetActiveEvents();
    }
}
