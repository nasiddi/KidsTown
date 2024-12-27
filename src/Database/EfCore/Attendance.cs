using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore;

public class Attendance
{
    public Attendance()
    {
        SearchLog2Attendances = new HashSet<SearchLog2Attendance>();
    }

    public int Id { get; set; }

    public long CheckInsId { get; set; }

    public int PersonId { get; set; }

    public int LocationId { get; set; }

    public string SecurityCode { get; set; } = null!;

    public int AttendanceTypeId { get; set; }

    public DateTime InsertDate { get; set; }

    public DateTime? CheckInDate { get; set; }

    public DateTime? CheckOutDate { get; set; }

    public virtual AttendanceType AttendanceType { get; set; } = null!;

    public virtual Location Location { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<SearchLog2Attendance> SearchLog2Attendances { get; set; }
}