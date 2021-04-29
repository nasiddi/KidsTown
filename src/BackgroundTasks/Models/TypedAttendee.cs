using KidsTown.Shared;

namespace KidsTown.BackgroundTasks.Models
{
    public class TypedAttendee
    {
        public readonly long PeopleId;
        public readonly AttendanceTypeId AttendanceTypeId;

        public TypedAttendee(long peopleId, AttendanceTypeId attendanceTypeId)
        {
            PeopleId = peopleId;
            AttendanceTypeId = attendanceTypeId;
        }
    }
}