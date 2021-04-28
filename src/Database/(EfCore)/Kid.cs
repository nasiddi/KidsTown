using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Kid
    {
        public int PersonId { get; set; }
        public int PersonTypeId { get; set; }
        public bool MayLeaveAlone { get; set; }
        public bool HasPeopleWithoutPickupPermission { get; set; }
        public DateTime UpdateDate { get; set; }

        public virtual Person Person { get; set; }
    }
}
