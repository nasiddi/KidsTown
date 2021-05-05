using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Family
    {
        public Family()
        {
            People = new HashSet<Person>();
        }

        public int Id { get; set; }
        public long HouseholdId { get; set; }
        public string Name { get; set; }
        public DateTime UpdateDate { get; set; }

        public virtual ICollection<Person> People { get; set; }
    }
}
