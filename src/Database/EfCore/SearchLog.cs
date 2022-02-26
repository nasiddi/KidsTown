using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class SearchLog
    {
        public SearchLog()
        {
            SearchLog2Attendances = new HashSet<SearchLog2Attendance>();
            SearchLog2LocationGroups = new HashSet<SearchLog2LocationGroup>();
        }

        public int Id { get; set; }
        public DateTime SearchDate { get; set; }
        public string SecurityCode { get; set; }
        public string DeviceGuid { get; set; }
        public bool IsCheckIn { get; set; }
        public long EventId { get; set; }

        public virtual ICollection<SearchLog2Attendance> SearchLog2Attendances { get; set; }
        public virtual ICollection<SearchLog2LocationGroup> SearchLog2LocationGroups { get; set; }
    }
}
