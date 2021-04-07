using System.Collections.Immutable;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

namespace KidsTown.IntegrationTests.TestData
{
    public class TestData
    {
        public readonly string CheckInsFirstName;
        public readonly string CheckInsLastName;
        public readonly string? PeopleFirstName;
        public readonly string? PeopleLastName;
        public readonly long CheckInsId;
        public readonly long? PeopleId;
        public readonly AttendeeType AttendanceType;
        public readonly TestLocationIds TestLocation;
        public readonly IImmutableList<TestFieldData> FieldData;
        public readonly bool? ExpectedMayLeaveAlone;
        public readonly bool? ExpectedHasPeopleWithoutPickupPermission;

        public string SecurityCode => $"{TestLocation.ToString().Substring(startIndex: 0, length: 1)}{CheckInsId}{AttendanceType.ToString().Substring(startIndex: 0, length: 1)}{PeopleId ?? 0}";
        
        public TestData(
            string checkInsFirstName,
            string checkInsLastName,
            string peopleFirstName,
            string peopleLastName,
            long checkInsId,
            long? peopleId,
            AttendeeType attendanceType,
            TestLocationIds testLocation,
            IImmutableList<TestFieldData> fieldData,
            bool expectedMayLeaveAlone,
            bool expectedHasPeopleWithoutPickupPermission
        )
        {
            CheckInsFirstName = checkInsFirstName;
            CheckInsLastName = checkInsLastName;
            PeopleFirstName = peopleFirstName;
            PeopleLastName = peopleLastName;
            CheckInsId = checkInsId;
            PeopleId = peopleId;
            AttendanceType = attendanceType;
            TestLocation = testLocation;
            ExpectedMayLeaveAlone = expectedMayLeaveAlone;
            ExpectedHasPeopleWithoutPickupPermission = expectedHasPeopleWithoutPickupPermission;
            FieldData = fieldData;
        }
        
        public TestData(
            string checkInsFirstName,
            string checkInsLastName,
            long checkInsId,
            long? peopleId,
            AttendeeType attendanceType,
            TestLocationIds testLocation
        )
        {
            CheckInsFirstName = checkInsFirstName;
            CheckInsLastName = checkInsLastName;
            CheckInsId = checkInsId;
            PeopleId = peopleId;
            AttendanceType = attendanceType;
            TestLocation = testLocation;
            FieldData = ImmutableList<TestFieldData>.Empty;
        }
    }
}