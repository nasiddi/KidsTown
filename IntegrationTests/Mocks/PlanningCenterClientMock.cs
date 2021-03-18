using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.PlanningCenterAPIClient;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.EventResult;
using CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult;
using Attendee = CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult.Attendee;
using Datum = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Datum;
using Included = CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult.Included;
using IncludedAttributes = CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult.IncludedAttributes;
using Parent = CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Parent;
using Relationship = CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult.Relationship;

namespace IntegrationTests.Mocks
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
            public readonly TestLocations TestLocation;

            public AttendanceData(
                string firstName, 
                string lastName, 
                long checkInId, 
                long? peopleId, 
                AttendeeType attendanceType,
                TestLocations testLocation)
            {
                FirstName = firstName;
                LastName = lastName;
                CheckInId = checkInId;
                PeopleId = peopleId;
                AttendanceType = attendanceType;
                TestLocation = testLocation;
            }
        }

        public class PersonData
        {
            public readonly string FirstName;
            public readonly string LastName;
            public readonly long PeopleId;
            public readonly IImmutableList<long> FieldDataIds;

            public PersonData(
                string firstName, 
                string lastName, 
                long peopleId, IImmutableList<long> fieldDataIds)
            {
                FirstName = firstName;
                LastName = lastName;
                PeopleId = peopleId;
                FieldDataIds = fieldDataIds;
            }
        }
        
        public static ImmutableList<AttendanceData> GetAttendanceData()
        {
            return ImmutableList.Create(
                new AttendanceData(
                    firstName: "Hanna", 
                    lastName: "Hase", 
                    checkInId: 1, 
                    peopleId: 1, 
                    attendanceType: AttendeeType.Regular,
                    testLocation: TestLocations.Haesli),
                
                new AttendanceData(
                    firstName: "Sarah", 
                    lastName: "Schaf", 
                    checkInId: 2, 
                    peopleId: null, 
                    attendanceType: AttendeeType.Guest,
                    testLocation: TestLocations.Schoefli),
                
                new AttendanceData(
                    firstName: "Frida", 
                    lastName: "Fuchs", 
                    checkInId: 3, 
                    peopleId: 3, 
                    attendanceType: AttendeeType.Volunteer,
                    testLocation: TestLocations.Fuechsli)
                );
        }

        public static ImmutableList<PersonData> GetPersonData()
        {
            return ImmutableList.Create(
                new PersonData(
                    firstName: "Hanna",
                    lastName: "Osterhase",
                    peopleId: 1,
                    fieldDataIds: ImmutableList.Create(item: (long) 1)),

                new PersonData(
                    firstName: "Frida",
                    lastName: "Fuchser",
                    peopleId: 3,
                    fieldDataIds: ImmutableList.Create(item: (long) 3))
                );
        }
        
        

        
        public Task<CheckIns> GetCheckedInPeople(int daysLookBack)
        {
            var data = GetAttendanceData();
            return Task.FromResult(result: new CheckIns
            {
                Attendees = GetAttendees(data: data),
                Included = GetCheckInIncluded()
            });
        }

        public Task<People> GetPeopleUpdates(IImmutableList<long> peopleIds)
        {
            var data = GetPersonData();
            
            return Task.FromResult(result: new People
            {
                Data = GetPeopleData(data: data),
                Included = GetPeopleIncluded()
            });
        }

        public Task<Event> GetActiveEvents()
        {
            return Task.FromResult(result: new Event
            {
                Data = GetEventData()
            });
        }

        private static List<CheckInsExtension.PlanningCenterAPIClient.Models.EventResult.Datum> GetEventData()
        {
            return new()
            {
                new CheckInsExtension.PlanningCenterAPIClient.Models.EventResult.Datum
                {
                    Id = 0,
                    Attributes = new Attributes
                    {
                        Name = null
                    }
                }
            };
        }

        private static List<Datum> GetPeopleData(ImmutableList<PersonData> data)
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

        private static List<CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included> GetPeopleIncluded()
        {
            return new()
            {
                new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included
                {
                    PeopleIncludedType = PeopleIncludedType.FieldDatum,
                    Id = 1,
                    Attributes = new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.IncludedAttributes
                    {
                        Value = null
                    },
                    Relationships = new IncludedRelationships
                    {
                        FieldDefinition = new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Relationship
                        {
                            Data = new Parent
                            {
                                Id = (long) FieldIds.None
                            }
                        }
                    }
                },
                new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included
                {
                    PeopleIncludedType = PeopleIncludedType.FieldDatum,
                    Id = 2,
                    Attributes = new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.IncludedAttributes
                    {
                        Value = null
                    },
                    Relationships = new IncludedRelationships
                    {
                        FieldDefinition = new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Relationship
                        {
                            Data = new Parent
                            {
                                Id = (long) FieldIds.None
                            }
                        }
                    }
                },
                new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Included
                {
                    PeopleIncludedType = PeopleIncludedType.FieldDatum,
                    Id = 3,
                    Attributes = new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.IncludedAttributes
                    {
                        Value = null
                    },
                    Relationships = new IncludedRelationships
                    {
                        FieldDefinition = new CheckInsExtension.PlanningCenterAPIClient.Models.PeopleResult.Relationship
                        {
                            Data = new Parent
                            {
                                Id = (long) FieldIds.None
                            }
                        }
                    }
                }
            };
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
                    SecurityCode =
                        $"{d.TestLocation.ToString().Substring(startIndex: 0, length: 1)}{d.CheckInId}{d.AttendanceType.ToString().Substring(startIndex: 0, length: 1)}{d.PeopleId ?? 0}"
                },
                Relationships = new AttendeeRelationships
                {
                    Locations = GetLocationParent(testLocation: d.TestLocation),
                    Person = d.PeopleId.HasValue ? GetPersonParent(id: d.PeopleId.Value) : null,
                    Event = GetTestEventParent()
                }
            }).ToList();
        }

        private static Locations GetLocationParent(TestLocations testLocation) =>
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
                    Id = (long) TestLocations.Haesli,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Häsli Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.Schoefli,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Schöfli Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.Fuechsli,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Füchsli Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.KidsChurch1St,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 1. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.KidsChurch2Nd,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 2. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.KidsChurch3Rd,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 3. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.KidsChurch4Th,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 4. Klasse Test"
                    }
                },
                new Included
                {
                    Type = IncludeType.Location,
                    Id = (long) TestLocations.KidsChurch5Th,
                    Attributes = new IncludedAttributes
                    {
                        Name = "Kids Church 5. Klasse Test"
                    }
                }
            };
        }

        private enum FieldIds
        {
            None = 0
            //NeedsToBePickedUp = 438360,
            //Kab = 441655
        }

        public enum TestLocations
        {
            Haesli = 788187,
            Schoefli = 790575,
            Fuechsli = 788188,
            KidsChurch1St = 787349,
            KidsChurch2Nd = 803333,
            KidsChurch3Rd = 803334,
            KidsChurch4Th = 803335,
            KidsChurch5Th = 803336
        }
    }
}