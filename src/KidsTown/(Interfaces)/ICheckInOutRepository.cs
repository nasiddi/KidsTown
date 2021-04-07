using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface ICheckInOutRepository
    {
        Task<ImmutableList<Person>> GetPeople(PeopleSearchParameters peopleSearchParameters);
        Task<bool> CheckInPeople(IImmutableList<int> checkInIds);
        Task<bool> CheckOutPeople(IImmutableList<int> checkInIds);
        Task<bool> SetCheckState(CheckState revertedCheckState, ImmutableList<int> checkInIds);
        Task<int> CreateGuest(int locationId, string securityCode, string firstName, string lastName);
    }
}