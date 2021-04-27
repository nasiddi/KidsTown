﻿namespace KidsTown.BackgroundTasks.Models
{
    public class PeopleUpdate
    {
        public readonly long? PeopleId;
        public readonly long? HouseholdId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string? HouseholdName;
        public readonly bool MayLeaveAlone;
        public readonly bool HasPeopleWithoutPickupPermission;

        public PeopleUpdate(
            long? peopleId,
            long? householdId,
            string firstName, 
            string lastName,
            string? householdName,
            bool mayLeaveAlone,
            bool hasPeopleWithoutPickupPermission)
        {
            PeopleId = peopleId;
            HouseholdId = householdId;
            FirstName = firstName;
            LastName = lastName;
            HouseholdName = householdName;
            MayLeaveAlone = mayLeaveAlone;
            HasPeopleWithoutPickupPermission = hasPeopleWithoutPickupPermission;
        }
    }
}