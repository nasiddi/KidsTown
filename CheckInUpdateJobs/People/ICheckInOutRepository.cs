using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface ICheckInOutRepository
    {
        Task<ImmutableList<Person>> GetPeople(PeopleSearchParameters peopleSearchParameters);
        Task<bool> CheckInPeople(IImmutableList<int> checkInIds);
        Task<bool> CheckOutPeople(IImmutableList<int> checkInIds);
        Task<bool> SetCheckState(CheckState revertedCheckState, ImmutableList<int> checkInIds);
    }
}