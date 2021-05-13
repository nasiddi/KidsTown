using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class Adult
    {
        public int PersonId { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPrimaryContact { get; set; }
        public long? PhoneNumberId { get; set; }

        public virtual Person Person { get; set; }
    }
}
