using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using Person = KidsTown.KidsTown.Models.Person;

namespace KidsTown.KidsTown
{
    public interface ICheckInOutService
    {
        Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters);
        Task<bool> CheckInOutPeople(CheckType checkType, IImmutableList<int> attendanceIds);
        Task<bool> UndoAction(CheckState revertedCheckState, ImmutableList<int> attendanceIds);
        Task<int?> CreateGuest(int locationId, string securityCode, string firstName, string lastName);
    }
}