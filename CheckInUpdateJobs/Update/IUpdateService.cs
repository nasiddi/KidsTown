using System.Threading.Tasks;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public interface IUpdateService
    {
        Task FetchDataFromPlanningCenter();
    }
}