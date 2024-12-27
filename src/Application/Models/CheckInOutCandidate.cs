namespace KidsTown.Application.Models;

public record CheckInOutCandidate(
    int AttendanceId,
    string Name,
    int LocationId,
    bool MayLeaveAlone,
    bool HasPeopleWithoutPickupPermission);