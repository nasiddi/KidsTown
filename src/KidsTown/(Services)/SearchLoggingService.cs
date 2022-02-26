using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown.KidsTown
{
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
            CheckType checkType
        )
        {
            try
            {
                await _searchLoggingRepository.LogSearch(peopleSearchParameters, people, deviceGuid, checkType);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}