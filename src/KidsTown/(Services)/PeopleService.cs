using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public class PeopleService : IPeopleService
    {
        private readonly IPeopleRepository _peopleRepository;
        public PeopleService(IPeopleRepository peopleRepository)
        {
            _peopleRepository = peopleRepository;
        }

        public async Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds)
        {
            return await _peopleRepository.GetParents(attendanceIds);
        }
        public async Task UpdateAdults(IImmutableList<Adult> adults)
        {
            await _peopleRepository.UpdateAdults(adults);
        }
    }
}