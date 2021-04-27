using System;
using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class Adult
    {
        public int Id { get; set; }
        public long PeopleId { get; set; }
        public int FamilyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime UpdateDate { get; set; }

        public virtual Family Family { get; set; }
    }
}
