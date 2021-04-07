namespace KidsTown.Application.Models
{
    public class GuestCheckInRequest
    { 
        public string SecurityCode { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public int LocationId { get; init; }
    }
}