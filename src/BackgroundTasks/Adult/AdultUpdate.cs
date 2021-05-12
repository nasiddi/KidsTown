namespace KidsTown.BackgroundTasks.Adult
{
    public class AdultUpdate
    {
        public readonly long PeopleId;
        public readonly int FamilyId;
        public readonly long PhoneNumberId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string PhoneNumber;

        public AdultUpdate(long peopleId, int familyId, long phoneNumberId, string firstName, string lastName, string phoneNumber)
        {
            PeopleId = peopleId;
            FamilyId = familyId;
            PhoneNumberId = phoneNumberId;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }
    }
}