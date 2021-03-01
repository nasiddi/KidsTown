using System;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;

namespace CheckInsExtension.CheckInUpdateJobs.Update
{
    public class CheckInUpdate
    {
        public readonly long CheckInId;
        public readonly long? PeopleId;
        public readonly AttendeeTypeEnum AttendeeType;
        public readonly string SecurityCode;
        public readonly string Location;
        public readonly DateTime CreationDate;
        public readonly PeopleUpdate Person;

        public CheckInUpdate(
            long checkInId, 
            long? peopleId, 
            AttendeeTypeEnum attendeeType,
            string securityCode, 
            string location, 
            DateTime creationDate,
            PeopleUpdate person)
        {
            CheckInId = checkInId;
            PeopleId = peopleId;
            SecurityCode = securityCode;
            Location = location;
            CreationDate = creationDate;
            Person = person;
            AttendeeType = attendeeType;
        }
    }
}