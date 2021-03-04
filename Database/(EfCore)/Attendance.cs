using System;

#nullable disable

namespace ChekInsExtension.Database
{
    public class Attendance
    {
        public int Id { get; set; }
        public long CheckInId { get; set; }
        public long EventId { get; set; }
        public int PersonId { get; set; }
        public int LocationId { get; set; }
        public string SecurityCode { get; set; }
        public int AttendanceTypeId { get; set; }
        public DateTime InsertDate { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }

        public virtual AttendanceType AttendanceType { get; set; }
        public virtual Location Location { get; set; }
        public virtual Person Person { get; set; }
    }
}
