using System.Collections.Immutable;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using Person = CheckInsExtension.CheckInUpdateJobs.Models.Person;

namespace CheckInsExtension.CheckInUpdateJobs.People
{
    public interface ICheckInOutService
    {
        Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters);
        Task<bool> CheckInPeople(IImmutableList<int> checkInIds);
        Task<bool> CheckOutPeople(IImmutableList<int> checkInIds);
        Task<bool> UndoAction(CheckState revertedCheckState, ImmutableList<int> checkinIds);
    }
}