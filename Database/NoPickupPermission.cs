using System;
using System.Collections.Generic;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class NoPickupPermission
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string Name { get; set; }

        public virtual Person Person { get; set; }
    }
}
