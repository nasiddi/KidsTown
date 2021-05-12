using System;
using KidsTown.Shared;

namespace KidsTown.BackgroundTasks.Common
{
    public class TypedAttendee
    {
        public readonly long PeopleId;
        public readonly AttendanceTypeId AttendanceTypeId;
        public readonly DateTime UpdateDate;

        public TypedAttendee(long peopleId, AttendanceTypeId attendanceTypeId, DateTime updateDate)
        {
            PeopleId = peopleId;
            AttendanceTypeId = attendanceTypeId;
            UpdateDate = updateDate;
        }
    }
}