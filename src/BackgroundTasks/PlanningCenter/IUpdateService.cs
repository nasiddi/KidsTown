using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.PlanningCenter
{
    public interface IUpdateService
    {
        Task FetchDataFromPlanningCenter();
    }
}