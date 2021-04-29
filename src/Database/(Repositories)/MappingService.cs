using KidsTown.Shared;

namespace KidsTown.Database
{
    public static class MappingService
    {
        public static CheckState GetCheckState(Attendance attendance)
        {
            var checkState = CheckState.PreCheckedIn;

            if (attendance.CheckInDate.HasValue)
            {
                checkState = CheckState.CheckedIn;
            }

            if (attendance.CheckOutDate.HasValue)
            {
                checkState = CheckState.CheckedOut;
            }

            return checkState;
        }
    }
}