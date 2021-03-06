using System.Collections.Immutable;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class PeopleSearchParameters
    {
        public readonly string SecurityCode;
        public readonly long EventId;
        public readonly IImmutableList<int> Locations;

        public PeopleSearchParameters(string securityCode,  long eventId, IImmutableList<int> locations)
        {
            SecurityCode = securityCode;
            Locations = locations;
            EventId = eventId;
        }
    }
}