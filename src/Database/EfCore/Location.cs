using System.Collections.Generic;

namespace KidsTown.Database.EfCore;

public class Location
{
    public Location()
    {
        Attendances = new HashSet<Attendance>();
    }

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int LocationGroupId { get; set; }

    public long? CheckInsLocationId { get; set; }

    public long EventId { get; set; }

    public virtual LocationGroup LocationGroup { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; }
}