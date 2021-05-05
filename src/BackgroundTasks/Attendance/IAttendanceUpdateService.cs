using System.Threading.Tasks;

namespace KidsTown.BackgroundTasks.Attendance
{
    public interface IAttendanceUpdateService
    {
        public Task<int> UpdateAttendance(int daysLookBack);
    }
}