using System.Collections.Immutable;

namespace KidsTown.KidsTown.Models
{
    public class PeopleSearchParameters
    {
        public readonly string SecurityCode;
        public readonly long EventId;
        public readonly IImmutableList<int> LocationGroups;

        public PeopleSearchParameters(string securityCode,  long eventId, IImmutableList<int> locationGroups)
        {
            SecurityCode = securityCode;
            LocationGroups = locationGroups;
            EventId = eventId;
        }
    }
}