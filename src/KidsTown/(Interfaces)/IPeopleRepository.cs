using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IPeopleRepository
    {
        Task<IImmutableList<Adult>> GetParents(IImmutableList<int> attendanceIds);
        Task<IImmutableList<Adult>> GetAdults(IImmutableList<int> familyIds);
    }
}