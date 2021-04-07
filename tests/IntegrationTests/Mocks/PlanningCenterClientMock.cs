using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.IntegrationTests.TestData;
using KidsTown.PlanningCenterApiClient;
using KidsTown.PlanningCenterApiClient.Models.CheckInResult;
using KidsTown.PlanningCenterApiClient.Models.EventResult;
using KidsTown.PlanningCenterApiClient.Models.PeopleResult;
using Attendee = KidsTown.PlanningCenterApiClient.Models.CheckInResult.Attendee;
using Datum = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Datum;
using Included = KidsTown.PlanningCenterApiClient.Models.CheckInResult.Included;
using IncludedAttributes = KidsTown.PlanningCenterApiClient.Models.CheckInResult.IncludedAttributes;
using Parent = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Parent;
using Relationship = KidsTown.PlanningCenterApiClient.Models.CheckInResult.Relationship;

namespace KidsTown.IntegrationTests.Mocks
{
    public class PlanningCenterClientMock : IPlanningCenterClient
    {
        public class AttendanceData
        {
            public readonly string FirstName;
            public readonly string LastName;
            public readonly long CheckInId;
            public readonly long? PeopleId;
            public readonly AttendeeType AttendanceType;
            public readonly TestLocationIds TestLocation;
            public readonly string SecurityCode;

            public AttendanceData(
                string firstName,
                string lastName,
                long checkInId,
                long? peopleId,
                AttendeeType attendanceType,
                TestLocationIds testLocation,
                string securityCode
            )
            {
                FirstName = firstName;
                LastName = lastName;
                CheckInId = checkInId;
                PeopleId = peopleId;
                AttendanceType = attendanceType;
                TestLocation = testLocation;
                SecurityCode = securityCode;
            }
        }

        public class PersonData
        {
            public readonly string FirstName;
            public readonly string LastName;
            public readonly long PeopleId;
            public readonly IImmutableList<long> FieldDataIds;
            public readonly bool? MayLeaveAlone;
            public readonly bool? HasPeopleWithoutPickupPermission;

            public PersonData(
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

        public static ImmutableList<AttendanceData> GetAttendanceData()
        {
            return TestDataFactory.GetTestData().Select(selector: d =>
                new AttendanceData(
                    firstName: d.CheckInFirstName,
                    lastName: d.CheckInLastName,
                    checkInId: d.CheckInId,
                    peopleId: d.PeopleId,
                    attendanceType: d.AttendanceType,
                    testLocation: d.TestLocation,
                    securityCode: d.SecurityCode
                )
            ).ToImmutableList();
        }

        public static ImmutableList<PersonData> GetPersonData()
        {
            return TestDataFactory.GetTestData()
                .Where(predicate: d => d.PeopleId.HasValue)
                .GroupBy(keySelector: d => d.PeopleId)
                .Select(selector: d =>
                    {
                        var person = d.First();
                        return new PersonData(
                            firstName: person.PeopleFirstName!,
                            lastName: person.PeopleLastName!,
                            peopleId: person.PeopleId!.Value,
                            fieldDataIds: person.FieldData.Select(selector: f => f.FieldOptionId).ToImmutableList(),
                            mayLeaveAlone: person.ExpectedMayLeaveAlone,
                            hasPeopleWithoutPickupPermission: person.ExpectedHasPeopleWithoutPickupPermission);
                    }
                ).ToImmutableList();
        }

        public Task<ImmutableList<CheckIns>> GetCheckedInPeople(int daysLookBack)
        {
            var data = GetAttendanceData();
            return Task.FromResult(result: ImmutableList.Create(
                item: new CheckIns
                {
                    Attendees = GetAttendees(data: data),
                    Included = GetCheckInIncluded()
                }));
        }

        public Task<ImmutableList<People>> GetPeopleUpdates(IImmutableList<long> peopleIds)
        {
            var data = GetPersonData();

            return Task.FromResult(result: ImmutableList.Create(
                item: new People
                {
                    Data = MapPersonData(data: data),
                    Included = GetPeopleIncluded()
                }));
        }

        public Task<Event> GetActiveEvents()
        {
            return Task.FromResult(result: new Event
            {
                Data = GetEventData()
            });
        }

        private static List<PlanningCenterApiClient.Models.EventResult.Datum> GetEventData()
        {
            return new()
            {
                new PlanningCenterApiClient.Models.EventResult.Datum
                {
                    Id = 0,
                    Attributes = new Attributes
                    {
                        Name = null
                    }
                }
            };
        }

        private static List<Datum> MapPersonData(ImmutableList<PersonData> data)
        {
            return data.Select(selector: d => new Datum
            {
                Id = d.PeopleId,
                Attributes = new DatumAttributes
                {
                    FirstName = d.FirstName,
                    LastName = d.LastName
                },
                Relationships = new DatumRelationships
                {
                    FieldData = new FieldData
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
            var fieldData = TestDataFactory.GetTestData()
                .Where(predicate: d => d.PeopleId.HasValue)
                .SelectMany(selector: d => d.FieldData)
                .ToImmutableList();

            return fieldData.GroupBy(keySelector: f => f.FieldOptionId)
                .Select(selector: f =>
                    {
                        var field = f.Last();
                        return new PlanningCenterApiClient.Models.PeopleResult.Included
                        {
                            PeopleIncludedType = PeopleIncludedType.FieldDatum,
                            Id = field.FieldOptionId,
                            Attributes =
                                new PlanningCenterApiClient.Models.PeopleResult.IncludedAttributes
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
                    }
                ).ToList();
        }

        private static List<Attendee> GetAttendees(ImmutableList<AttendanceData> data)
        {
            var createdAt = DateTime.UtcNow;

            return data.Select(selector: d => new Attendee
            {
                Id = d.CheckInId,
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
                    Locations = GetLocationParent(testLocation: d.TestLocation),
                    Person = d.PeopleId.HasValue ? GetPersonParent(id: d.PeopleId.Value) : null,
                    Event = GetTestEventParent()
                }
            }).ToList();
        }

        private static Locations GetLocationParent(TestLocationIds testLocation) =>
            new()
            {
                Data = new List<ParentElement>
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
                Data = new ParentElement
                {
                    Id = 389697
                }
            };

        private static Relationship GetPersonParent(long id) =>
            new()
            {
                Data = new ParentElement
                {
                    Id = id
                }
            };

        private static List<Included> GetCheckInIncluded()
        {
            return new()
            {
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.Haesli,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Häsli Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.Schoefli,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Schöfli Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.Fuechsli,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Füchsli Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.KidsChurch1St,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 1. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.KidsChurch2Nd,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 2. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.KidsChurch3Rd,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 3. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.KidsChurch4Th,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 4. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocationIds.KidsChurch5Th,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 5. Klasse Test"
                    }
                }
            };
        }
    }
}