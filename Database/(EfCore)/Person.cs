using System;
using System.Collections.Generic;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class Person
    {
        public Person()
        {
            CheckIns = new HashSet<CheckIn>();
            NoPickupPermissions = new HashSet<NoPickupPermission>();
        }

        public int Id { get; set; }
        public long? PeopleId { get; set; }
        public string FistName { get; set; }
        public string LastName { get; set; }
        public bool? MayLeaveAlone { get; set; }
        public bool? HasPeopleWithoutPickupPermission { get; set; }

        public virtual ICollection<CheckIn> CheckIns { get; set; }
        public virtual ICollection<NoPickupPermission> NoPickupPermissions { get; set; }
    }
}
