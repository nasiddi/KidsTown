using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public interface IUpdateService
    {
        Task<int> FetchDataFromPlanningCenter();
        void LogTaskRun(bool success, int updateCount, string environment);
    }
}