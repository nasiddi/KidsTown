// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace KidsTown.KidsTown.Models
{
    public class Adult
    {
        public long? PeopleId { get; init; }
        public long? PhoneNumberId { get; init; }
        public int PersonId { get; init; }
        public int FamilyId { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public bool IsPrimaryContact { get; init; }
    }
}