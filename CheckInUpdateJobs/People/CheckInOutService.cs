using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public class CheckInOutService : ICheckInOutService
    {
        private readonly ICheckInOutRepository _checkInOutRepository;
        private readonly IConfigurationService _configurationService;

        public CheckInOutService(ICheckInOutRepository checkInOutRepository, IConfigurationService configurationService)
        {
            _checkInOutRepository = checkInOutRepository;
            _configurationService = configurationService;
        }
        
        public async Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters)
        {
            return await _checkInOutRepository.GetPeople(searchParameters);
        }

        public async Task<bool> CheckInPeople(IImmutableList<int> checkInIds)
        {
            return await _checkInOutRepository.CheckInPeople(checkInIds);
        }

        public async Task<bool> CheckOutPeople(IImmutableList<int> checkInIds)
        {
            return await _checkInOutRepository.CheckOutPeople(checkInIds);
        }
    }
}