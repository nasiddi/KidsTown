using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public interface IUpdateRepository
    {
        Task InsertPreCheckIns(IImmutableList<CheckInUpdate> preCheckIns);
        Task<IImmutableList<long>> GetExistingCheckInIds(IImmutableList<long> checkinIds);
        Task<ImmutableList<long>> GetPeopleIdsPreCheckedIns(int daysLookBack);
        Task UpdatePersons(ImmutableList<PeopleUpdate> peoples);
    }
}