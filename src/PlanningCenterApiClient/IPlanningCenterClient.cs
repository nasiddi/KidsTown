using System.Collections.Immutable;
using PlanningCenterApiClient.Models.CheckInsResult;
using PlanningCenterApiClient.Models.EventResult;
using PlanningCenterApiClient.Models.HouseholdResult;
using PlanningCenterApiClient.Models.PeopleResult;

namespace PlanningCenterApiClient;

public interface IPlanningCenterClient
{
    Task<IImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack);
    Task<IImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds);
    Task<Household?> GetHousehold(long householdId);
    Task<Event?> GetActiveEvents();
    Task PatchPhoneNumber(long peopleId, long phoneNumberId, string phoneNumber);
    Task PostPhoneNumber(long peopleId, string phoneNumber);
}