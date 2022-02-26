using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class SearchLog2Attendance
    {
        public int Id { get; set; }
        public int SearchLogId { get; set; }
        public int AttendanceId { get; set; }

        public virtual Attendance Attendance { get; set; }
        public virtual SearchLog SearchLog { get; set; }
    }
}
