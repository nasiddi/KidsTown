namespace KidsTown.Application.Models
{
    public class CheckInOutCandidate
    {
        public int CheckInId { get; init; }
        public string Name { get; init; } = string.Empty;
        public bool MayLeaveAlone { get; init; } = true;
        public bool HasPeopleWithoutPickupPermission { get; init; }
    }
}