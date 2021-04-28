using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface ICheckInOutService
    {
        Task<IImmutableList<Kid>> SearchForPeople(PeopleSearchParameters searchParameters);
        Task<bool> CheckInOutPeople(CheckType checkType, IImmutableList<int> attendanceIds);
        Task<bool> UndoAction(CheckState revertedCheckState, IImmutableList<int> attendanceIds);
        Task<int?> CreateGuest(int locationId, string securityCode, string firstName, string lastName);
    }
}