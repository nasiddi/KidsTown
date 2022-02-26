using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class SearchLog2LocationGroup
    {
        public int Id { get; set; }
        public int SearchLogId { get; set; }
        public int LocationGroupId { get; set; }

        public virtual LocationGroup LocationGroup { get; set; }
        public virtual SearchLog SearchLog { get; set; }
    }
}
