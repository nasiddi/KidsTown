using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore
{
    public partial class LocationGroup
    {
        public LocationGroup()
        {
            Locations = new HashSet<Location>();
            SearchLog2LocationGroups = new HashSet<SearchLog2LocationGroup>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsEnabled { get; set; }

        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<SearchLog2LocationGroup> SearchLog2LocationGroups { get; set; }
    }
}
