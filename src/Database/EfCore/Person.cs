using System;
using System.Collections.Generic;

#nullable disable

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
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime UpdateDate { get; set; }

        public virtual Family Family { get; set; }
        public virtual Adult Adult { get; set; }
        public virtual Kid Kid { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
