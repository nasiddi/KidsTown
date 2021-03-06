using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface ICheckInOutRepository
    {
        Task<ImmutableList<Person>> GetPeople(PeopleSearchParameters peopleSearchParameters,
            IImmutableList<long> eventIds);
        Task<bool> CheckInPeople(IImmutableList<int> checkInIds);
        Task<bool> CheckOutPeople(IImmutableList<int> checkInIds);
    }
}