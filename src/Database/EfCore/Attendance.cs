﻿using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class Attendance
    {
        public Attendance()
        {
            SearchLog2Attendances = new HashSet<SearchLog2Attendance>();
        }

        public int Id { get; set; }
        public long CheckInsId { get; set; }
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
        public virtual ICollection<SearchLog2Attendance> SearchLog2Attendances { get; set; }
    }
}
