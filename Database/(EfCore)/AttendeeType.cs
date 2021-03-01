using System;
using System.Collections.Generic;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class AttendeeType
    {
        public AttendeeType()
        {
            CheckIns = new HashSet<CheckIn>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CheckIn> CheckIns { get; set; }
    }
}
