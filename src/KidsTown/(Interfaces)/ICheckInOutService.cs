using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using Person = KidsTown.KidsTown.Models.Person;

namespace KidsTown.KidsTown
{
    public interface ICheckInOutService
    {
        Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters);
        Task<bool> CheckInPeople(IImmutableList<int> checkInIds);
        Task<bool> CheckOutPeople(IImmutableList<int> checkInIds);
        Task<bool> UndoAction(CheckState revertedCheckState, ImmutableList<int> checkinIds);
        Task<int?> CheckInGuest(int locationId, string securityCode, string firstName, string lastName);
    }
}