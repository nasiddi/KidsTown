using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Person
    {
        public Person()
        {
            Attendances = new HashSet<Attendance>();
        }

        public int Id { get; set; }
        public long? PeopleId { get; set; }
        public string FistName { get; set; }
        public string LastName { get; set; }
        public bool MayLeaveAlone { get; set; }
        public bool HasPeopleWithoutPickupPermission { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
