using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Adult
    {
        public int PersonId { get; set; }
        public string PhoneNumber { get; set; }

        public virtual Person Person { get; set; }
    }
}
