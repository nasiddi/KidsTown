// ReSharper disable UnusedAutoPropertyAccessor.Global

using KidsTown.Shared;

namespace KidsTown.Models;

public class Kid
{
    public int AttendanceId { get; init; }

    public int LocationGroupId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public bool MayLeaveAlone { get; init; }

    public bool HasPeopleWithoutPickupPermission { get; init; }

    public CheckState CheckState { get; init; }
}