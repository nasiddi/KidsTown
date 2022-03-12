using System.Collections.Immutable;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.Shared;

namespace KidsTown.IntegrationTests.TestData;

public static class TestDataFactory
{
    public static IImmutableList<TestData> GetTestData()
    {
        var testData = ImmutableList.Create(
            new TestData(
                checkInsFirstName: "Hanna",
                checkInsLastName: "Hase",
                peopleFirstName: "Hanna",
                peopleLastName: "Osterhase",
                checkInsId: 1,
                peopleId: 1,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.Haesli,
                fieldData: ImmutableList<TestFieldData>.Empty,
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 1,
                householdName: "Hase Household"
            ),
            new TestData(
                checkInsFirstName: "Sarah",
                checkInsLastName: "Schaf",
                checkInsId: 2,
                peopleId: null,
                attendanceType: AttendeeType.Guest,
                testLocation: TestLocationIds.Schoefli,
                securityCode: "2S2S"
            ),
            new TestData(
                checkInsFirstName: "Frida",
                checkInsLastName: "Fuchs",
                checkInsId: 3,
                peopleId: 3,
                attendanceType: AttendeeType.Volunteer,
                testLocation: TestLocationIds.Fuechsli,
                peopleFirstName: "Frida",
                peopleLastName: "Fuchser",
                fieldData: ImmutableList.Create(
                    item: new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.NeedsToBePickedUp,
                        fieldOptionId: 3,
                        value: "false")),
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 3,
                householdName: "Fuchs Household"
            ),
            new TestData(
                checkInsFirstName: "Hans",
                checkInsLastName: "Hase",
                checkInsId: 4,
                peopleId: 4,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.Haesli,
                peopleFirstName: "Hans",
                peopleLastName: "Hase",
                fieldData: ImmutableList.Create(
                    item: new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.NeedsToBePickedUp,
                        fieldOptionId: 4,
                        value: "true")),
                expectedMayLeaveAlone: false,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 1,
                householdName: "Hase Household"
            ),
            new TestData(
                checkInsFirstName: "Sandro",
                checkInsLastName: "Schaf",
                checkInsId: 5,
                peopleId: 5,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.Schoefli,
                peopleFirstName: "Sandro",
                peopleLastName: "Schaf",
                fieldData: ImmutableList.Create(
                    item: new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.Kab,
                        fieldOptionId: 5,
                        value: "true")),
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: true,
                securityCode: "2S2S",
                householdId: 2,
                householdName: "Schaf Household"
            ),
            new TestData(
                checkInsFirstName: "Franz",
                checkInsLastName: "Fox",
                checkInsId: 6,
                peopleId: 6,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.Fuechsli,
                peopleFirstName: "Franz",
                peopleLastName: "Fox",
                fieldData: ImmutableList.Create(
                    new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.Kab,
                        fieldOptionId: 6,
                        value: "true"),
                    new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.NeedsToBePickedUp,
                        fieldOptionId: 7,
                        value: "false")),
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: true,
                householdId: 3,
                householdName: "Fuchs Household"
            ),
            new TestData(
                checkInsFirstName: "Henry",
                checkInsLastName: "Hasenfuss",
                checkInsId: 7,
                peopleId: 7,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.Haesli,
                peopleFirstName: "Henry",
                peopleLastName: "Hasenfuss",
                fieldData: ImmutableList.Create(
                    new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.Kab,
                        fieldOptionId: 8,
                        value: null),
                    new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.NeedsToBePickedUp,
                        fieldOptionId: 9,
                        value: null)),
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 1,
                householdName: "Hase Household",
                securityCode: "1H1H"
            ),
            new TestData(
                checkInsFirstName: "Henry",
                checkInsLastName: "Hasenfuss",
                checkInsId: 8,
                peopleId: 7,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.Haesli,
                peopleFirstName: "Henry",
                peopleLastName: "Hasenfuss",
                fieldData: ImmutableList.Create(
                    new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.Kab,
                        fieldOptionId: 8,
                        value: null),
                    new TestFieldData(
                        fieldDefinitionId: PeopleFieldId.NeedsToBePickedUp,
                        fieldOptionId: 9,
                        value: null)),
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 1,
                householdName: "Hase Household",
                securityCode: "1H1H"
            ),
            new TestData(
                checkInsFirstName: "Ernst",
                checkInsLastName: "Erster",
                checkInsId: 9,
                peopleId: 9,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.KidsChurch1St,
                peopleFirstName: "Ernst",
                peopleLastName: "Erster",
                fieldData: ImmutableList<TestFieldData>.Empty, 
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 4,
                householdName: "KidsChurch Household"
            ),
            new TestData(
                checkInsFirstName: "Zara",
                checkInsLastName: "Zweiter",
                checkInsId: 10,
                peopleId: 10,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.KidsChurch2Nd,
                peopleFirstName: "Zara",
                peopleLastName: "Zweiter",
                fieldData: ImmutableList<TestFieldData>.Empty,
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 4,
                householdName: "KidsChurch Household"
            ),
            new TestData(
                checkInsFirstName: "Daniel",
                checkInsLastName: "Dritter",
                checkInsId: 11,
                peopleId: 11,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.KidsChurch3Rd,
                peopleFirstName: "Daniel",
                peopleLastName: "Dritter",
                fieldData: ImmutableList<TestFieldData>.Empty,
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 4,
                householdName: "KidsChurch Household"
            ),
            new TestData(
                checkInsFirstName: "Vreni",
                checkInsLastName: "Vierter",
                checkInsId: 12,
                peopleId: 12,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.KidsChurch4Th,
                peopleFirstName: "Vreni",
                peopleLastName: "Vierter",
                fieldData: ImmutableList<TestFieldData>.Empty,
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 4,
                householdName: "KidsChurch Household"
            ),
            new TestData(
                checkInsFirstName: "Freddie",
                checkInsLastName: "Fünfter",
                checkInsId: 13,
                peopleId: 13,
                attendanceType: AttendeeType.Regular,
                testLocation: TestLocationIds.KidsChurch5Th,
                peopleFirstName: "Freddie",
                peopleLastName: "Fünfter",
                fieldData: ImmutableList<TestFieldData>.Empty,
                expectedMayLeaveAlone: true,
                expectedHasPeopleWithoutPickupPermission: false,
                householdId: 4,
                householdName: "KidsChurch Household"
            )
        );

        return testData;
    }
}