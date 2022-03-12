using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore;

public partial class Kid
{
    public int PersonId { get; set; }
    public bool MayLeaveAlone { get; set; }
    public bool HasPeopleWithoutPickupPermission { get; set; }
    public DateTime UpdateDate { get; set; }

    public virtual Person Person { get; set; } = null!;
}