using System.Collections.Immutable;

namespace KidsTown.KidsTown.Models
{
    public class PeopleSearchParameters
    {
        public readonly string SecurityCode;
        public readonly long EventId;
        public readonly IImmutableList<int> LocationGroups;
        public readonly bool UseFilterLocationGroups;

        public PeopleSearchParameters(
            string securityCode,
            long eventId,
            IImmutableList<int> locationGroups,
            bool useFilterLocationGroups
        )
        {
            SecurityCode = securityCode;
            LocationGroups = locationGroups;
            UseFilterLocationGroups = useFilterLocationGroups;
            EventId = eventId;
        }
    }
}