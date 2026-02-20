using System.Collections.Immutable;
using BackgroundTasks.Adult;
using BackgroundTasks.Common;

namespace BackgroundTasks.Kid;

public interface IKidUpdateRepository
{
    Task<ImmutableList<long>> GetKidsPeopleIdToUpdate(int daysLookBack, int take);
    Task<int> UpdateKids(IImmutableList<PeopleUpdate> kids, IImmutableList<Family> families);
    Task<IImmutableList<Family>> GetExistingFamilies(IImmutableList<long> householdIds);
    Task<IImmutableList<Family>> InsertFamilies(IImmutableList<long> newHouseholdIds, IImmutableList<PeopleUpdate> peoples);
}