namespace BackgroundTasks.Attendance;

public interface IAttendanceUpdateService
{
    public Task<int> UpdateAttendance(int daysLookBack);
}