using System.Collections.Immutable;
using PlanningCenterApiClient.Models.CheckInsResult;

namespace IntegrationTests.TestData;

public class TestData
{
    public readonly AttendeeType AttendanceType;
    public readonly string CheckInsFirstName;
    public readonly long CheckInsId;
    public readonly string CheckInsLastName;
    public readonly bool? ExpectedHasPeopleWithoutPickupPermission;
    public readonly bool? ExpectedMayLeaveAlone;
    public readonly IImmutableList<TestFieldData> FieldData;
    public readonly long? HouseholdId;
    public readonly string? HouseholdName;
    public readonly int LocationGroupId;
    public readonly string? PeopleFirstName;
    public readonly long? PeopleId;
    public readonly string? PeopleLastName;
    public readonly string SecurityCode;
    public readonly TestLocationIds TestLocation;

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
        long householdId,
        string householdName,
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

        HouseholdId = householdId;
        HouseholdName = householdName;

        SecurityCode = SetSecurityCode(securityCode);
        LocationGroupId = GetLocationGroup(testLocation);
    }

    public TestData(
        string checkInsFirstName,
        string checkInsLastName,
        long checkInsId,
        long? peopleId,
        AttendeeType attendanceType,
        TestLocationIds testLocation,
        string? securityCode
    )
    {
        CheckInsFirstName = checkInsFirstName;
        CheckInsLastName = checkInsLastName;
        CheckInsId = checkInsId;
        PeopleId = peopleId;
        AttendanceType = attendanceType;
        TestLocation = testLocation;
        SecurityCode = SetSecurityCode(securityCode);
        FieldData = ImmutableList<TestFieldData>.Empty;

        LocationGroupId = GetLocationGroup(testLocation);
    }

    private string SetSecurityCode(string? securityCode)
    {
        return securityCode ?? $"{TestLocation.ToString()[..1]}{CheckInsId}{AttendanceType.ToString()[..1]}{PeopleId ?? 0}";
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
            _ => throw new ArgumentOutOfRangeException(
                nameof(testLocationId),
                testLocationId,
                message: null)
        };
    }
}