using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IPeopleService
{
    Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds);
    Task UpdateAdults(IImmutableList<Adult> adults, bool updatePhoneNumber);
}