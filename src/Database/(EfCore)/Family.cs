using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Family
    {
        public Family()
        {
            Adults = new HashSet<Adult>();
            Kids = new HashSet<Kid>();
        }

        public int Id { get; set; }
        public long HouseholdId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Adult> Adults { get; set; }
        public virtual ICollection<Kid> Kids { get; set; }
    }
}
