using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.Kid
{
    public interface IKidUpdateService
    {
        public Task<int> UpdateKids(int daysLookBack);
    }
}