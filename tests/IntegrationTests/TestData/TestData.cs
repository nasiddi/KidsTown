using System.Collections.Immutable;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;

namespace IntegrationTests.TestData
{
    public class TestData
    {
        public readonly string CheckInFirstName;
        public readonly string CheckInLastName;
        public readonly string? PeopleFirstName;
        public readonly string? PeopleLastName;
        public readonly long CheckInId;
        public readonly long? PeopleId;
        public readonly AttendeeType AttendanceType;
        public readonly TestLocationIds TestLocation;
        public readonly IImmutableList<TestFieldData> FieldData;
        public readonly bool? ExpectedMayLeaveAlone;
        public readonly bool? ExpectedHasPeopleWithoutPickupPermission;

        public string SecurityCode => $"{TestLocation.ToString().Substring(startIndex: 0, length: 1)}{CheckInId}{AttendanceType.ToString().Substring(startIndex: 0, length: 1)}{PeopleId ?? 0}";
        
        public TestData(
            string checkInFirstName,
            string checkInLastName,
            string peopleFirstName,
            string peopleLastName,
            long checkInId,
            long? peopleId,
            AttendeeType attendanceType,
            TestLocationIds testLocation,
            IImmutableList<TestFieldData> fieldData,
            bool expectedMayLeaveAlone,
            bool expectedHasPeopleWithoutPickupPermission
        )
        {
            CheckInFirstName = checkInFirstName;
            CheckInLastName = checkInLastName;
            PeopleFirstName = peopleFirstName;
            PeopleLastName = peopleLastName;
            CheckInId = checkInId;
            PeopleId = peopleId;
            AttendanceType = attendanceType;
            TestLocation = testLocation;
            ExpectedMayLeaveAlone = expectedMayLeaveAlone;
            ExpectedHasPeopleWithoutPickupPermission = expectedHasPeopleWithoutPickupPermission;
            FieldData = fieldData;
        }
        
        public TestData(
            string checkInFirstName,
            string checkInLastName,
            long checkInId,
            long? peopleId,
            AttendeeType attendanceType,
            TestLocationIds testLocation
        )
        {
            CheckInFirstName = checkInFirstName;
            CheckInLastName = checkInLastName;
            CheckInId = checkInId;
            PeopleId = peopleId;
            AttendanceType = attendanceType;
            TestLocation = testLocation;
            FieldData = ImmutableList<TestFieldData>.Empty;
        }
    }
}