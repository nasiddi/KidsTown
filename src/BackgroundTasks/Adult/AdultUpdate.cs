namespace KidsTown.BackgroundTasks.Adult
{
    public class AdultUpdate
    {
        public readonly long PeopleId;
        public readonly int FamilyId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string PhoneNumber;

        public AdultUpdate(long peopleId, int familyId, string firstName, string lastName, string phoneNumber)
        {
            PeopleId = peopleId;
            FamilyId = familyId;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }
    }
}