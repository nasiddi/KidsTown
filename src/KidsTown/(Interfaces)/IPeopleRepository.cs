using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IPeopleRepository
{
    Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds);
    Task<IImmutableList<Adult>> GetAdults(IImmutableList<int> familyIds);
    Task UpdateAdults(IImmutableList<Adult> adults);
    Task InsertUnregisteredGuest(string securityCode, int locationId);
}