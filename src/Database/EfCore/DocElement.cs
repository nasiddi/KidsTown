using System;
using System.Collections.Generic;

namespace KidsTown.Database.EfCore;

public class DocElement
{
    public DocElement()
    {
        DocImages = new HashSet<DocImage>();
        DocParagraphs = new HashSet<DocParagraph>();
    }

    public int Id { get; set; }

    public int? PreviousId { get; set; }

    public int SectionId { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual DocElement? Previous { get; set; }

    public virtual DocTitle DocTitle { get; set; } = null!;

    public virtual DocElement? InversePrevious { get; set; }

    public virtual ICollection<DocImage> DocImages { get; set; }

    public virtual ICollection<DocParagraph> DocParagraphs { get; set; }
}