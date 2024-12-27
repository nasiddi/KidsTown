using System.Collections.Generic;

namespace KidsTown.Database.EfCore;

public class AttendanceType
{
    public AttendanceType()
    {
        Attendances = new HashSet<Attendance>();
    }

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; }
}