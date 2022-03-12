using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown;

public interface IPeopleService
{
    Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds);
    Task UpdateAdults(IImmutableList<Adult> adults, bool updatePhoneNumber);
}