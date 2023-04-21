using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore
{
    public partial class DocImage
    {
        public int Id { get; set; }
        public int? PreviousId { get; set; }
        public int ElementId { get; set; }
        public string FileId { get; set; } = null!;

        public virtual DocElement Element { get; set; } = null!;
        public virtual DocImage? Previous { get; set; }
        public virtual DocImage? InversePrevious { get; set; }
    }
}
