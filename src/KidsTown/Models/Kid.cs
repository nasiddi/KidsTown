// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace KidsTown.KidsTown.Models
{
    public class Kid
    {
        public int AttendanceId { get; init; }
        public string SecurityCode { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public DateTime? CheckInTime { get; init; }
        public DateTime? CheckOutTime { get; init; }
        public bool MayLeaveAlone { get; init; }    
        public bool HasPeopleWithoutPickupPermission { get; init; }    
        public CheckState CheckState { get; init; }
    }
}