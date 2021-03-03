using System.Collections.Immutable;
using System.Linq;
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

        public async Task<ImmutableList<Location>> GetActiveLocations()
        {
            return await _checkInOutRepository.GetActiveLocations();
        }

        public async Task<ImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocations)
        {
            return await _checkInOutRepository.GetActiveAttendees(selectedLocations);
        }
    }
}