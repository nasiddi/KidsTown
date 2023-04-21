using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore
{
    public partial class DocParagraph
    {
        public int Id { get; set; }
        public int? PreviousId { get; set; }
        public int ElementId { get; set; }
        public string Content { get; set; } = null!;
        public int? ParagraphIconId { get; set; }

        public virtual DocElement Element { get; set; } = null!;
        public virtual DocParagraph? Previous { get; set; }
        public virtual DocParagraph? InversePrevious { get; set; }
    }
}
