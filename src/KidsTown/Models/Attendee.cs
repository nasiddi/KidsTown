using System;
using System.Collections.Immutable;
using KidsTown.Shared;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace KidsTown.KidsTown.Models
{
    public class Attendee
    {
        public int AttendanceId { get; init; }
        public int? FamilyId { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public AttendanceTypeId AttendanceTypeId { get; init; }
        public int LocationGroupId { get; init; }
        public string Location { get; init; } = string.Empty;
        public string SecurityCode { get; init; } = string.Empty;
        public CheckState CheckState { get; init; }
        public DateTime InsertDate { get; init; }
        public DateTime? CheckInDate { get; init; }
        public DateTime? CheckOutDate { get; init; }
        public IImmutableList<Adult> Adults { get; init; } = ImmutableList<Adult>.Empty;
    }
}