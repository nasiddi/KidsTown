﻿namespace KidsTown.Database.EfCore;

public class SearchLog2Attendance
{
    public int Id { get; set; }

    public int SearchLogId { get; set; }

    public int AttendanceId { get; set; }

    public virtual Attendance Attendance { get; set; } = null!;

    public virtual SearchLog SearchLog { get; set; } = null!;
}