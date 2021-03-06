using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.EventResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;

namespace CheckInsExtension.PlanningCenterAPIClient
{
    public interface IPlanningCenterClient
    {
        Task<CheckIns> GetCheckedInPeople(int daysLookBack);
        Task<People> GetPeopleUpdates(IImmutableList<long> peopleIds);
        Task<EventResult> GetActiveEvents();
    }
}
