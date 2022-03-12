using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Common;

namespace KidsTown.BackgroundTasks.Kid;

public interface IKidUpdateRepository
{
    Task<ImmutableList<long>> GetKidsPeopleIdToUpdate(int daysLookBack, int take);
    Task<int> UpdateKids(IImmutableList<PeopleUpdate> kids, IImmutableList<Family> families);
    Task<IImmutableList<Family>> GetExistingFamilies(IImmutableList<long> householdIds);
    Task<IImmutableList<Family>> InsertFamilies(IImmutableList<long> newHouseholdIds, IImmutableList<PeopleUpdate> peoples);
}