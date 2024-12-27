using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.PlanningCenterApiClient;

namespace KidsTown.KidsTown;

public class PeopleService(IPeopleRepository peopleRepository, IPlanningCenterClient planningCenterClient)
    : IPeopleService
{
    public async Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds)
    {
        return await peopleRepository.GetParents(attendanceIds);
    }

    public async Task UpdateAdults(IImmutableList<Adult> adults, bool updatePhoneNumber)
    {
        if (updatePhoneNumber)
        {
            foreach (var adult in adults)
            {
                if (!adult.PeopleId.HasValue)
                {
                    continue;
                }

                if (adult.PhoneNumberId.HasValue)
                {
                    await planningCenterClient.PatchPhoneNumber(
                        adult.PeopleId.Value,
                        adult.PhoneNumberId.Value,
                        adult.PhoneNumber);

                    continue;
                }

                await planningCenterClient.PostPhoneNumber(
                    adult.PeopleId.Value,
                    adult.PhoneNumber);
            }
        }

        await peopleRepository.UpdateAdults(adults);
    }
}