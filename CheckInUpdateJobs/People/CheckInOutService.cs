using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public class CheckInOutService : ICheckInOutService
    {
        private readonly ICheckInOutRepository _checkInOutRepository;

        public CheckInOutService(ICheckInOutRepository checkInOutRepository)
        {
            _checkInOutRepository = checkInOutRepository;
        }
        
        public async Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters)
        {
            return await _checkInOutRepository.GetPeople(peopleSearchParameters: searchParameters).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<bool> CheckInPeople(IImmutableList<int> checkInIds)
        {
            return await _checkInOutRepository.CheckInPeople(checkInIds: checkInIds).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<bool> CheckOutPeople(IImmutableList<int> checkInIds)
        {
            return await _checkInOutRepository.CheckOutPeople(checkInIds: checkInIds).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}