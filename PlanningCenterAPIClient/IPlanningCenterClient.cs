using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.EventResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;

namespace CheckInsExtension.PlanningCenterAPIClient
{
    public interface IPlanningCenterClient
    {
        Task<ImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack);
        Task<ImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds);
        Task<Event> GetActiveEvents();
    }
}
