using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown.KidsTown;

public class SearchLoggingService : ISearchLoggingService
{
    private readonly ISearchLoggingRepository _searchLoggingRepository;

    public SearchLoggingService(ISearchLoggingRepository searchLoggingRepository)
    {
        _searchLoggingRepository = searchLoggingRepository;
    }
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
            await _searchLoggingRepository.LogSearch(
                peopleSearchParameters: peopleSearchParameters,
                people: people,
                deviceGuid: deviceGuid,
                checkType: checkType,
                filterLocations: filterLocations);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}