// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KidsTown.Application.Models
{
    public class GuestCheckInRequest
    { 
        public string SecurityCode { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public int LocationGroupId { get; init; }
    }
}