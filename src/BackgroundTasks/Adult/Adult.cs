namespace KidsTown.BackgroundTasks.Adult
{
    public class Adult
    {
        public readonly long PeopleId;
        public readonly int PersonId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string PhoneNumber;

        public Adult(long peopleId, int personId, string firstName, string lastName, string phoneNumber)
        {
            PeopleId = peopleId;
            PersonId = personId;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }
    }
}