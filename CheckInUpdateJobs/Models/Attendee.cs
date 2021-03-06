using System;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class Attendee
    {
        public string Name;
        public AttendanceTypes AttendanceType;
        public int LocationId;
        public CheckState CheckState;
        public DateTime InsertDate;
        public DateTime? CheckInDate;
        public DateTime? CheckOutDate;

        public Attendee(
            string name, 
            AttendanceTypes attendanceType, 
            int locationId, 
            CheckState checkState, 
            DateTime insertDate, 
            DateTime? checkInDate, 
            DateTime? checkOutDate)
        {
            Name = name;
            AttendanceType = attendanceType;
            LocationId = locationId;
            CheckState = checkState;
            InsertDate = insertDate;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }
    }
}