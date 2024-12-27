namespace KidsTown.Database.EfCore;

public class SearchLog2LocationGroup
{
    public int Id { get; set; }

    public int SearchLogId { get; set; }

    public int LocationGroupId { get; set; }

    public virtual LocationGroup LocationGroup { get; set; } = null!;

    public virtual SearchLog SearchLog { get; set; } = null!;
}