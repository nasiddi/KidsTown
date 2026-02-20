using System.Collections.Immutable;
using KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown;

public class SearchLoggingService(ISearchLoggingRepository searchLoggingRepository) : ISearchLoggingService
{
    public async Task LogSearch(
        PeopleSearchParameters peopleSearchParameters,
        IImmutableList<Kid> people,
        string deviceGuid,
        CheckType checkType,
        bool filterLocations
    )
    {
        try
        {
            await searchLoggingRepository.LogSearch(
                peopleSearchParameters,
                people,
                deviceGuid,
                checkType,
                filterLocations);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}