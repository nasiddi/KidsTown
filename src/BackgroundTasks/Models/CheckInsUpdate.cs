using System;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

namespace KidsTown.BackgroundTasks.Models
{
    public class CheckInsUpdate
    {
        public readonly long CheckInsId;
        public readonly long? PeopleId;
        public readonly AttendeeType AttendeeType;
        public readonly string SecurityCode;
        public readonly int LocationId;
        public readonly DateTime CreationDate;
        public readonly PeopleUpdate Person;

        public CheckInsUpdate(
            long checkInsId, 
            long? peopleId, 
            AttendeeType attendeeType,
            string securityCode,
            int locationId, 
            DateTime creationDate,
            PeopleUpdate person)
        {
            CheckInsId = checkInsId;
            PeopleId = peopleId;
            SecurityCode = securityCode;

            LocationId = locationId;
            CreationDate = creationDate;
            Person = person;
            AttendeeType = attendeeType;
        }
    }
}