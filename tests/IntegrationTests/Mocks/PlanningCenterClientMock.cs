using System.Collections.Immutable;
using IntegrationTests.TestData;
using PlanningCenterApiClient;
using PlanningCenterApiClient.Models.CheckInsResult;
using PlanningCenterApiClient.Models.EventResult;
using PlanningCenterApiClient.Models.HouseholdResult;
using PlanningCenterApiClient.Models.PeopleResult;
using Datum = PlanningCenterApiClient.Models.EventResult.Datum;
using Included = PlanningCenterApiClient.Models.PeopleResult.Included;
using IncludedAttributes = PlanningCenterApiClient.Models.PeopleResult.IncludedAttributes;
using Relationship = PlanningCenterApiClient.Models.CheckInsResult.Relationship;

namespace IntegrationTests.Mocks;

public class PlanningCenterClientMock : IPlanningCenterClient
{
    public Task<IImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack)
    {
        var data = GetAttendanceData();
        var result = ImmutableList.Create(
            new CheckIns
            {
                Attendees = GetAttendees(data),
                Included = GetCheckInsIncluded()
            });

        return Task.FromResult(result as IImmutableList<CheckIns>);
    }

    public Task<IImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds)
    {
        var data = GetKidsData();
        var result = ImmutableList.Create(
            new People
            {
                Data = MapPersonData(data),
                Included = GetPeopleIncluded()
            });

        return Task.FromResult(result as IImmutableList<People>);
    }

    public Task<Household?> GetHousehold(long householdId)
    {
        return Task.FromResult<Household?>(result: null);
    }

    public Task<Event?> GetActiveEvents()
    {
        return Task.FromResult(
            new Event
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

    public static IImmutableList<AttendanceData> GetAttendanceData()
    {
        return TestDataFactory.GetTestData()
            .Select(
                d =>
                    new AttendanceData(
                        d.CheckInsFirstName,
                        d.CheckInsLastName,
                        d.CheckInsId,
                        d.PeopleId,
                        d.AttendanceType,
                        d.TestLocation,
                        d.SecurityCode))
            .ToImmutableList();
    }

    public static IImmutableList<KidsData> GetKidsData()
    {
        return TestDataFactory.GetTestData()
            .Where(d => d.PeopleId.HasValue)
            .GroupBy(d => d.PeopleId)
            .Select(
                d =>
                {
                    var kid = d.First();
                    return new KidsData(
                        kid.PeopleFirstName!,
                        kid.PeopleLastName!,
                        kid.PeopleId!.Value,
                        kid.FieldData.Select(f => f.FieldOptionId).ToImmutableList(),
                        kid.ExpectedMayLeaveAlone,
                        kid.ExpectedHasPeopleWithoutPickupPermission);
                })
            .ToImmutableList();
    }

    private static List<Datum> GetEventData()
    {
        return new List<Datum>
        {
            new()
            {
                Id = 0,
                Attributes = new Attributes
                {
                    Name = null
                }
            }
        };
    }

    private static List<PlanningCenterApiClient.Models.PeopleResult.Datum> MapPersonData(IImmutableList<KidsData> data)
    {
        return data.Select(
                d => new PlanningCenterApiClient.Models.PeopleResult.Datum
                {
                    Id = d.PeopleId,
                    Attributes = new DatumAttributes
                    {
                        FirstName = d.FirstName,
                        LastName = d.LastName
                    },
                    Relationships = new DatumRelationships
                    {
                        FieldData = new DatumRelationship
                        {
                            Data = GetFieldDataParents(d.FieldDataIds)
                        }
                    }
                })
            .ToList();
    }

    private static List<Parent> GetFieldDataParents(IImmutableList<long> ids)
    {
        return ids.Select(
                i => new Parent
                {
                    Id = i
                })
            .ToList();
    }

    private static List<Included> GetPeopleIncluded()
    {
        var testData = TestDataFactory.GetTestData();

        var fieldData = testData.Where(d => d.PeopleId.HasValue)
            .SelectMany(d => d.FieldData)
            .ToImmutableList();

        var fieldIncluded = fieldData.GroupBy(f => f.FieldOptionId)
            .Select(
                f =>
                {
                    var field = f.Last();
                    return new Included
                    {
                        PeopleIncludedType = PeopleIncludedType.FieldDatum,
                        Id = field.FieldOptionId,
                        Attributes =
                            new IncludedAttributes
                            {
                                Value = field.Value
                            },
                        Relationships = new IncludedRelationships
                        {
                            FieldDefinition =
                                new PlanningCenterApiClient.Models.PeopleResult.Relationship
                                {
                                    Data = new Parent
                                    {
                                        Id = (long) field.FieldDefinitionId
                                    }
                                }
                        }
                    };
                });

        var householdIncluded = testData
            .Where(t => t.HouseholdId.HasValue)
            .Select(
                t => new Included
                {
                    PeopleIncludedType = PeopleIncludedType.Household,
                    Id = t.HouseholdId!.Value,
                    Attributes = new IncludedAttributes
                    {
                        Name = t.HouseholdName
                    },
                    Relationships = null
                });

        return fieldIncluded.Union(householdIncluded).ToList();
    }

    private static List<Attendee> GetAttendees(IImmutableList<AttendanceData> data)
    {
        var createdAt = DateTime.UtcNow;

        return data.Select(
                d => new Attendee
                {
                    Id = d.CheckInsId,
                    Attributes = new AttendeeAttributes
                    {
                        CreatedAt = createdAt,
                        FirstName = d.FirstName,
                        Kind = d.AttendanceType,
                        LastName = d.LastName,
                        SecurityCode = d.SecurityCode
                    },
                    Relationships = new AttendeeRelationships
                    {
                        Locations = GetLocationParent(d.TestLocation),
                        Person = d.PeopleId.HasValue ? GetPersonParent(d.PeopleId.Value) : null,
                        Event = GetTestEventParent()
                    }
                })
            .ToList();
    }

    private static CheckInsLocations GetLocationParent(TestLocationIds testLocation)
    {
        return new CheckInsLocations
        {
            Data = new List<ParentElement>
            {
                new()
                {
                    Id = (long) testLocation
                }
            }
        };
    }

    private static Relationship GetTestEventParent()
    {
        return new Relationship
        {
            Data = new ParentElement
            {
                Id = 389697
            }
        };
    }

    private static Relationship GetPersonParent(long id)
    {
        return new Relationship
        {
            Data = new ParentElement
            {
                Id = id
            }
        };
    }

    private static List<PlanningCenterApiClient.Models.CheckInsResult.Included> GetCheckInsIncluded()
    {
        return new List<PlanningCenterApiClient.Models.CheckInsResult.Included>
        {
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.Haesli,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Häsli Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.Schoefli,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Schöfli Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.Fuechsli,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Füchsli Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch1St,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Kids Church 1. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch2Nd,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Kids Church 2. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch3Rd,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Kids Church 3. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch4Th,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Kids Church 4. Klasse Test"
                }
            },
            new()
            {
                Type = IncludeType.Location,
                Id = (long) TestLocationIds.KidsChurch5Th,
                Attributes = new PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes
                {
                    Name = "Kids Church 5. Klasse Test"
                }
            }
        };
    }

    public record AttendanceData(
        string FirstName,
        string LastName,
        long CheckInsId,
        long? PeopleId,
        AttendeeType AttendanceType,
        TestLocationIds TestLocation,
        string SecurityCode);

    public record KidsData(
        string FirstName,
        string LastName,
        long PeopleId,
        IImmutableList<long> FieldDataIds,
        bool? MayLeaveAlone,
        bool? HasPeopleWithoutPickupPermission);
}