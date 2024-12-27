namespace KidsTown.Database.EfCore;

public class DocTitle
{
    public int Id { get; set; }

    public int ElementId { get; set; }

    public string Content { get; set; } = null!;

    public int Size { get; set; }

    public virtual DocElement Element { get; set; } = null!;
}