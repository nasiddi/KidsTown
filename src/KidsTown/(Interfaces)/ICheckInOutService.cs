using System.Collections.Immutable;
using KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown;

public interface ICheckInOutService
{
    Task<IImmutableList<Kid>> SearchForPeople(PeopleSearchParameters searchParameters);
    Task<bool> CheckInOutPeople(CheckType checkType, IImmutableList<int> attendanceIds);
    Task<bool> UndoAction(CheckState revertedCheckState, IImmutableList<int> attendanceIds);
    Task<int?> CreateGuest(int locationId, string securityCode, string firstName, string lastName);
    Task CreateUnregisteredGuest(string requestSecurityCode, long requestEventId, IImmutableList<int> requestSelectedLocationIds);
    Task<bool> UpdateLocationAndCheckIn(int attendanceId, int locationId);
}