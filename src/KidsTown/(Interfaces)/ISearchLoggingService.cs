using System.Collections.Immutable;
using KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown;

public interface ISearchLoggingService
{
    Task LogSearch(
        PeopleSearchParameters peopleSearchParameters,
        IImmutableList<Kid> people,
        string deviceGuid,
        CheckType checkType,
        bool filterLocations
    );
}