using System.Collections.Generic;

#nullable disable

namespace KidsTown.Database
{
    public partial class AttendanceType
    {
        public AttendanceType()
        {
            Attendances = new HashSet<Attendance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
