using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown.KidsTown;

public interface ICheckInOutRepository
{
    Task<IImmutableList<Kid>> GetPeople(PeopleSearchParameters peopleSearchParameters);
    Task<bool> CheckInPeople(IImmutableList<int> attendanceIds);
    Task<bool> CheckOutPeople(IImmutableList<int> attendanceIds);
    Task<bool> SetCheckState(CheckState revertedCheckState, IImmutableList<int> attendanceIds);
    Task<int> CreateGuest(int locationId, string securityCode, string firstName, string lastName);
    Task<bool> SecurityCodeExists(string securityCode);
    Task<bool> UpdateLocationAndCheckIn(int attendanceId, int locationId);
}