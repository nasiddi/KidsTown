using System;
using System.Collections.Generic;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class Location
    {
        public Location()
        {
            CheckIns = new HashSet<CheckIn>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public virtual ICollection<CheckIn> CheckIns { get; set; }
    }
}
