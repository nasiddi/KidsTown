namespace KidsTown.Application.Models;

public class CheckInOutCandidate
{
    public int AttendanceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int LocationId { get; init; }
    public bool MayLeaveAlone { get; init; } = true;
    public bool HasPeopleWithoutPickupPermission { get; init; }
}