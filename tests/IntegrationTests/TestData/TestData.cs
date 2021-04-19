using System;
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
        public readonly string SecurityCode;
        public readonly int LocationGroupId;

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
            bool expectedHasPeopleWithoutPickupPermission,
            string? securityCode = null
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
            SecurityCode = SetSecurityCode(securityCode: securityCode);
            
            LocationGroupId = GetLocationGroup(testLocationId: testLocation);
        }

        public TestData(
            string checkInsFirstName,
            string checkInsLastName,
            long checkInsId,
            long? peopleId,
            AttendeeType attendanceType,
            TestLocationIds testLocation,
            string? securityCode = null
        )
        {
            CheckInsFirstName = checkInsFirstName;
            CheckInsLastName = checkInsLastName;
            CheckInsId = checkInsId;
            PeopleId = peopleId;
            AttendanceType = attendanceType;
            TestLocation = testLocation;
            SecurityCode = SetSecurityCode(securityCode: securityCode);
            FieldData = ImmutableList<TestFieldData>.Empty;

            LocationGroupId = GetLocationGroup(testLocationId: testLocation);
        }
        
        private string SetSecurityCode(string? securityCode)
        {
            return securityCode ?? $"{TestLocation.ToString().Substring(startIndex: 0, length: 1)}{CheckInsId}{AttendanceType.ToString().Substring(startIndex: 0, length: 1)}{PeopleId ?? 0}";
        }

        private static int GetLocationGroup(TestLocationIds testLocationId)
        {
            return testLocationId switch
            {
                TestLocationIds.Haesli => 1,
                TestLocationIds.Schoefli => 2,
                TestLocationIds.Fuechsli => 3,
                TestLocationIds.KidsChurch1St => 4,
                TestLocationIds.KidsChurch2Nd => 4,
                TestLocationIds.KidsChurch3Rd => 4,
                TestLocationIds.KidsChurch4Th => 4,
                TestLocationIds.KidsChurch5Th => 4,
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(testLocationId), actualValue: testLocationId, message: null)
            };
        }
    }
}