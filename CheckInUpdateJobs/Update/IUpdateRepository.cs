using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public interface IUpdateRepository
    {
        Task InsertPreCheckIns(IImmutableList<CheckInUpdate> preCheckIns);
        Task<IImmutableList<long>> GetExistingCheckInIds(IImmutableList<long> checkinIds);
        Task<ImmutableList<long>> GetPeopleIdsPreCheckedInToday();
        Task UpdatePersons(ImmutableList<PeopleUpdate> peoples);
    }
}