namespace Database.EfCore;

public class Family
{
    public Family()
    {
        People = new HashSet<Person>();
    }

    public int Id { get; set; }

    public long? HouseholdId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public virtual ICollection<Person> People { get; set; }
}