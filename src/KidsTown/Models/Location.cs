namespace KidsTown.Models;

public class Location(int id, string name, int locationGroupId)
{
    public readonly int Id = id;
    public readonly int LocationGroupId = locationGroupId;
    public readonly string Name = name;
}