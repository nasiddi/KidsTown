using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using Microsoft.Extensions.Configuration;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public class CheckInOutService : ICheckInOutService
    {
        private readonly ICheckInOutRepository _checkInOutRepository;
        private readonly IConfiguration _configuration;

        public CheckInOutService(ICheckInOutRepository checkInOutRepository, IConfiguration configuration)
        {
            _checkInOutRepository = checkInOutRepository;
            _configuration = configuration;
        }
        
        public async Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters)
        {
            return await _checkInOutRepository.GetPeople(searchParameters, GetEventIds());
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
            return await _checkInOutRepository.GetActiveAttendees(selectedLocations, GetEventIds());
        }

        private IImmutableList<long> GetEventIds()
        {
            return _configuration.GetSection("EventIds").Get<long[]>().ToImmutableList();
        }
    }
}