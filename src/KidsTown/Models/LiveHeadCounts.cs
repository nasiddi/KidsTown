// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.Models;

public class LiveHeadCounts
{
    public string Location { get; init; } = string.Empty;

    public int KidsCount { get; init; }

    public int VolunteersCount { get; init; }
}