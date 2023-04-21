using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore
{
    public partial class Person
    {
        public Person()
        {
            Attendances = new HashSet<Attendance>();
        }

        public int Id { get; set; }
        public int? FamilyId { get; set; }
        public long? PeopleId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime UpdateDate { get; set; }

        public virtual Family? Family { get; set; }
        public virtual Adult Adult { get; set; } = null!;
        public virtual Kid Kid { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
