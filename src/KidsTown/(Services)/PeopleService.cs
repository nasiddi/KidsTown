using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.PlanningCenterApiClient;

namespace KidsTown.KidsTown
{
    public class PeopleService : IPeopleService
    {
        private readonly IPeopleRepository _peopleRepository;
        private readonly IPlanningCenterClient _planningCenterClient;
        public PeopleService(IPeopleRepository peopleRepository, IPlanningCenterClient planningCenterClient)
        {
            _peopleRepository = peopleRepository;
            _planningCenterClient = planningCenterClient;
        }

        public async Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds)
        {
            return await _peopleRepository.GetParents(attendanceIds);
        }
        public async Task UpdateAdults(IImmutableList<Adult> adults, bool updatePhoneNumber)
        {
            if (updatePhoneNumber)
            {
                foreach (var adult in adults)
                {
                    if (!adult.PeopleId.HasValue || !adult.PhoneNumberId.HasValue)
                    {
                        continue;
                    }

                    await _planningCenterClient.PatchPhoneNumber(
                        peopleId: adult.PeopleId.Value, 
                        phoneNumberId: adult.PhoneNumberId.Value, 
                        phoneNumber: adult.PhoneNumber);
                }
            }
            await _peopleRepository.UpdateAdults(adults);
        }
    }
}