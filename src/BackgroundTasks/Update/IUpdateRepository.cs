using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public interface IUpdateRepository
    {
        Task InsertPreCheckIns(IImmutableList<CheckInUpdate> preCheckIns);
        Task<IImmutableList<long>> GetExistingCheckInIds(IImmutableList<long> checkinIds);
        Task<ImmutableList<long>> GetCurrentPeopleIds(int daysLookBack);
        Task UpdatePersons(ImmutableList<PeopleUpdate> peoples);
        Task AutoCheckInVolunteers();
        Task AutoCheckoutEveryoneByEndOfDay();
        Task<ImmutableList<PersistedLocation>> GetPersistedLocations();
        Task UpdateLocations(ImmutableList<LocationUpdate> locationUpdates);
        Task EnableUnknownLocationGroup();
    }
}