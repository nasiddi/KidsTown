using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.IntegrationTests.TestData;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using KidsTown.PlanningCenterApiClient.Models.HouseholdResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using Attendee = KidsTown.PlanningCenterApiClient.Models.CheckInsResult.Attendee;
using Datum = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Datum;
using Included = KidsTown.PlanningCenterApiClient.Models.CheckInsResult.Included;
using Parent = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Parent;
using People = KidsTown.PlanningCenterApiClient.Models.PeopleResult.People;
using Relationship = KidsTown.PlanningCenterApiClient.Models.CheckInsResult.Relationship;

namespace KidsTown.IntegrationTests.Mocks;

public class PlanningCenterClientMock : IPlanningCenterClient
{
    public class AttendanceData
    {
        public readonly string FirstName;
        public readonly string LastName;
        public readonly long CheckInsId;
        public readonly long? PeopleId;
        public readonly AttendeeType AttendanceType;
        public readonly TestLocationIds TestLocation;
        public readonly string SecurityCode;

        public AttendanceData(
            string firstName,
            string lastName,
            long checkInsId,
            long? peopleId,
            AttendeeType attendanceType,
            TestLocationIds testLocation,
            string securityCode
        )
        {
            FirstName = firstName;
            LastName = lastName;
            CheckInsId = checkInsId;
            PeopleId = peopleId;
            AttendanceType = attendanceType;
            TestLocation = testLocation;
            SecurityCode = securityCode;
        }
    }

    public class KidsData
    {
        public readonly string FirstName;
        public readonly string LastName;
        public readonly long PeopleId;
        public readonly IImmutableList<long> FieldDataIds;
        public readonly bool? MayLeaveAlone;
        public readonly bool? HasPeopleWithoutPickupPermission;

        public KidsData(
            string firstName,
            string lastName,
            long peopleId,
            IImmutableList<long> fieldDataIds,
            bool? mayLeaveAlone,
            bool? hasPeopleWithoutPickupPermission
        )
        {
            FirstName = firstName;
            LastName = lastName;
            PeopleId = peopleId;
            FieldDataIds = fieldDataIds;
            MayLeaveAlone = mayLeaveAlone;
            HasPeopleWithoutPickupPermission = hasPeopleWithoutPickupPermission;
        }
    }

    public static IImmutableList<AttendanceData> GetAttendanceData()
    {
        return TestDataFactory.GetTestData().Select(selector: d =>
            new AttendanceData(
                firstName: d.CheckInsFirstName,
                lastName: d.CheckInsLastName,
                checkInsId: d.CheckInsId,
                peopleId: d.PeopleId,
                attendanceType: d.AttendanceType,
                testLocation: d.TestLocation,
                securityCode: d.SecurityCode
            )
        ).ToImmutableList();
    }

    public static IImmutableList<KidsData> GetKidsData()
    {
        return TestDataFactory.GetTestData()
            .Where(predicate: d => d.PeopleId.HasValue)
            .GroupBy(keySelector: d => d.PeopleId)
            .Select(selector: d =>
                {
                    var kid = d.First();
                    return new KidsData(
                        firstName: kid.PeopleFirstName!,
                        lastName: kid.PeopleLastName!,
                        peopleId: kid.PeopleId!.Value,
                        fieldDataIds: kid.FieldData.Select(selector: f => f.FieldOptionId).ToImmutableList(),
                        mayLeaveAlone: kid.ExpectedMayLeaveAlone,
                        hasPeopleWithoutPickupPermission: kid.ExpectedHasPeopleWithoutPickupPermission);
                }
            ).ToImmutableList();
    }

    public Task<IImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack)
    {
        var data = GetAttendanceData();
        var result = ImmutableList.Create(
            item: new CheckIns
            {
                Attendees = GetAttendees(data: data),
                Included = GetCheckInsIncluded()
            });
            
        return Task.FromResult(result: result as IImmutableList<CheckIns>);
    }

    public Task<IImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds)
    {
        var data = GetKidsData();
        var result = ImmutableList.Create(
            item: new People
            {
                Data = MapPersonData(data: data),
                Included = GetPeopleIncluded()
            });
        return Task.FromResult(result: result as IImmutableList<People>);
    }

    public Task<Household?> GetHousehold(long householdId)
    {
        return Task.FromResult<Household?>(result: null);
    }

    public Task<Event?> GetActiveEvents()
    {
        return Task.FromResult(result: new Event
        {
            Data = GetEventData()
        })!;
    }
    public Task PatchPhoneNumber(long peopleId, long phoneNumberId, string phoneNumber)
    {
        return Task.CompletedTask;
    }
    public Task PostPhoneNumber(long peopleId, string phoneNumber)
    {
        return Task.CompletedTask;
    }

    private static List<PlanningCenterApiClient.Models.EventResult.Datum> GetEventData()
    {
        return new()
        {
            new()
            {
                Id = 0,
                Attributes = new()
                {
                    Name = null
                }
            }
        };
    }

    private static List<Datum> MapPersonData(IImmutableList<KidsData> data)
    {
        return data.Select(selector: d => new Datum
        {
            Id = d.PeopleId,
            Attributes = new()
            {
                FirstName = d.FirstName,
                LastName = d.LastName
            },
            Relationships = new()
            {
                FieldData = new()
                {
                    Data = GetFieldDataParents(ids: d.FieldDataIds)
                }
            }
        }).ToList();
    }

    private static List<Parent> GetFieldDataParents(IImmutableList<long> ids)
    {
        return ids.Select(selector: i => new Parent
        {
            Id = i
        }).ToList();
    }

    private static List<PlanningCenterApiClient.Models.PeopleResult.Included> GetPeopleIncluded()
    {
        var testData = TestDataFactory.GetTestData();
           
        var fieldData = testData.Where(predicate: d => d.PeopleId.HasValue)
            .SelectMany(selector: d => d.FieldData)
            .ToImmutableList();
        var fieldIncluded= fieldData.GroupBy(keySelector: f => f.FieldOptionId)
            .Select(selector: f =>
                {
                    var field = f.Last();
                    return new PlanningCenterApiClient.Models.PeopleResult.Included
                    {
                        PeopleIncludedType = PeopleIncludedType.FieldDatum,
                        Id = field.FieldOptionId,
                        Attributes =
                            new()
                            {
                                Value = field.Value
                            },
                        Relationships = new()
                        {
                            FieldDefinition =
                                new()
                                {
                                    Data = new()
                                    {
                                        Id = (long) field.FieldDefinitionId
                                    }
                                }
                        }
                    };
                }
            );


        var householdIncluded = testData
            .Where(predicate: t => t.HouseholdId.HasValue)
            .Select(selector: t => new PlanningCenterApiClient.Models.PeopleResult.Included
            {
                PeopleIncludedType = PeopleIncludedType.Household,
                Id = t.HouseholdId!.Value,
                Attributes = new()
                {
                    Name = t.HouseholdName
                },
                Relationships = null
            });

        return fieldIncluded.Union(second: householdIncluded).ToList();
    }

    private static List<Attendee> GetAttendees(IImmutableList<AttendanceData> data)
    {
        var createdAt = DateTime.UtcNow;

        return data.Select(selector: d => new Attendee
        {
            Id = d.CheckInsId,
            Attributes = new()
            {
                CreatedAt = createdAt,
                FirstName = d.FirstName,
                Kind = d.AttendanceType,
                LastName = d.LastName,
                SecurityCode = d.SecurityCode
            },
            Relationships = new()
            {
                Locations = GetLocationParent(testLocation: d.TestLocation),
                Person = d.PeopleId.HasValue ? GetPersonParent(id: d.PeopleId.Value) : null,
                Event = GetTestEventParent()
            }
        }).ToList();
    }

    private static CheckInsLocations GetLocationParent(TestLocationIds testLocation) =>
        new()
        {
            Data = new()
            {
                new()
                {
                    Id = (long) testLocation
                }
            }
        };

    private static Relationship GetTestEventParent() =>
        new()
        {
            Data = new()
            {
                Id = 389697
            }
        };

    private static Relationship GetPersonParent(long id) =>
        new()
        {
            Data = new()
            {
                Id = id
            }
        };

    private static List<Included> GetCheckInsIncluded()
    {
        return new()
        {
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.Haesli,
                Attributes = new()
                {
                    Name = "Häsli Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.Schoefli,
                Attributes = new()
                {
                    Name = "Schöfli Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.Fuechsli,
                Attributes = new()
                {
                    Name = "Füchsli Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch1St,
                Attributes = new()
                {
                    Name = "Kids Church 1. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch2Nd,
                Attributes = new()
                {
                    Name = "Kids Church 2. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch3Rd,
                Attributes = new()
                {
                    Name = "Kids Church 3. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch4Th,
                Attributes = new()
                {
                    Name = "Kids Church 4. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch5Th,
                Attributes = new()
                {
                    Name = "Kids Church 5. Klasse Test"
                }
            }
        };
    }
}