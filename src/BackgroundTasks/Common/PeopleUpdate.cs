namespace KidsTown.BackgroundTasks.Common
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
        
        public PeopleUpdate(
            long? peopleId,
            string firstName, 
            string lastName)
        {
            PeopleId = peopleId;
            FirstName = firstName;
            LastName = lastName;
            MayLeaveAlone = true;
            HasPeopleWithoutPickupPermission = false;
        }
    }
}