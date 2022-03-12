using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using People = KidsTown.PlanningCenterApiClient.Models.PeopleResult.People;

namespace KidsTown.PlanningCenterApiClient;

public interface IPlanningCenterClient
{
    Task<IImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack);
    Task<IImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds);
    Task<Household?> GetHousehold(long householdId);
    Task<Event?> GetActiveEvents();
    Task PatchPhoneNumber(long peopleId, long phoneNumberId, string phoneNumber);
    Task PostPhoneNumber(long peopleId, string phoneNumber);

}