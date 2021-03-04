using System;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class CheckInUpdate
    {
        public readonly long CheckInId;
        public readonly long? PeopleId;
        public readonly long EventId;
        public readonly AttendeeType AttendeeType;
        public readonly string SecurityCode;
        public readonly string Location;
        public readonly DateTime CreationDate;
        public readonly PeopleUpdate Person;

        public CheckInUpdate(
            long checkInId, 
            long? peopleId, 
            AttendeeType attendeeType,
            string securityCode, 
            string location, 
            DateTime creationDate,
            PeopleUpdate person, long eventId)
        {
            CheckInId = checkInId;
            PeopleId = peopleId;
            SecurityCode = securityCode;
            Location = location;
            CreationDate = creationDate;
            Person = person;
            EventId = eventId;
            AttendeeType = attendeeType;
        }
    }
}