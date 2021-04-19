using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Location
    {
        public Location()
        {
            Attendances = new HashSet<Attendance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int LocationGroupId { get; set; }
        public long? CheckInsLocationId { get; set; }
        public long EventId { get; set; }

        public virtual LocationGroup LocationGroup { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
