using System.Collections.Immutable;

namespace CheckInsExtension.CheckInUpdateJobs.Models
{
    public class PeopleSearchParameters
    {
        public readonly string SecurityCode;
        public readonly IImmutableList<int> Locations;

        public PeopleSearchParameters(string securityCode, IImmutableList<int> locations)
        {
            SecurityCode = securityCode;
            Locations = locations;
        }
    }
}