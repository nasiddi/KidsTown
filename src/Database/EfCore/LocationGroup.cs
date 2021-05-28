using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class LocationGroup
    {
        public LocationGroup()
        {
            Locations = new HashSet<Location>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public virtual ICollection<Location> Locations { get; set; }
    }
}
