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
using IncludedAttributes = KidsTown.PlanningCenterApiClient.Models.CheckInsResult.IncludedAttributes;
using IncludedRelationships = KidsTown.PlanningCenterApiClient.Models.PeopleResult.IncludedRelationships;
using Parent = KidsTown.PlanningCenterApiClient.Models.PeopleResult.Parent;
using People = KidsTown.PlanningCenterApiClient.Models.PeopleResult.People;
using Relationship = KidsTown.PlanningCenterApiClient.Models.CheckInsResult.Relationship;

namespace KidsTown.IntegrationTests.Mocks
{
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
            return TestDataFactory.GetTestData().Select(d =>
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
                .Where(d => d.PeopleId.HasValue)
                .GroupBy(d => d.PeopleId)
                .Select(d =>
                    {
                        var kid = d.First();
                        return new KidsData(
                            firstName: kid.PeopleFirstName!,
                            lastName: kid.PeopleLastName!,
                            peopleId: kid.PeopleId!.Value,
                            fieldDataIds: kid.FieldData.Select(f => f.FieldOptionId).ToImmutableList(),
                            mayLeaveAlone: kid.ExpectedMayLeaveAlone,
                            hasPeopleWithoutPickupPermission: kid.ExpectedHasPeopleWithoutPickupPermission);
                    }
                ).ToImmutableList();
        }

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
            return Task.FromResult<Household?>(null)!;
        }

        public Task<Event?> GetActiveEvents()
        {
            return Task.FromResult(new Event
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

        private static List<Datum> MapPersonData(IImmutableList<KidsData> data)
        {
            return data.Select(d => new Datum
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
            }).ToList();
        }

        private static List<Parent> GetFieldDataParents(IImmutableList<long> ids)
        {
            return ids.Select(i => new Parent
            {
                Id = i
            }).ToList();
        }

        private static List<PlanningCenterApiClient.Models.PeopleResult.Included> GetPeopleIncluded()
        {
            var testData = TestDataFactory.GetTestData();
           
            var fieldData = testData.Where(d => d.PeopleId.HasValue)
                .SelectMany(d => d.FieldData)
                .ToImmutableList();
            var fieldIncluded= fieldData.GroupBy(f => f.FieldOptionId)
                .Select(f =>
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
                );


            var householdIncluded = testData
                .Where(t => t.HouseholdId.HasValue)
                .Select(t => new PlanningCenterApiClient.Models.PeopleResult.Included
                {
                    PeopleIncludedType = PeopleIncludedType.Household,
                    Id = t.HouseholdId!.Value,
                    Attributes = new PlanningCenterApiClient.Models.PeopleResult.IncludedAttributes
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

            return data.Select(d => new Attendee
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
            }).ToList();
        }

        private static CheckInsLocations GetLocationParent(TestLocationIds testLocation) =>
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

        private static List<Included> GetCheckInsIncluded()
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