using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Cleanup;
using KidsTown.Database.EfCore;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Kid = KidsTown.KidsTown.Models.Kid;

namespace KidsTown.Database;

public class SearchLoggingRepository(IServiceScopeFactory serviceScopeFactory) : ISearchLoggingRepository,
    ISearchLogCleanupRepository
{
    public async Task<int> ClearOldLogs()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var searchLogs = await db.SearchLogs.Where(l => l.SearchDate < DateTime.Today.AddDays(-30))
            .Include(l => l.SearchLog2Attendances)
            .Include(l => l.SearchLog2LocationGroups)
            .ToListAsync();

        db.RemoveRange(searchLogs.SelectMany(l => l.SearchLog2Attendances));
        db.RemoveRange(searchLogs.SelectMany(l => l.SearchLog2LocationGroups));
        db.RemoveRange(searchLogs);

        return await db.SaveChangesAsync();
    }

    public async Task LogSearch(
        PeopleSearchParameters request,
        IImmutableList<Kid> people,
        string deviceGuid,
        CheckType checkType,
        bool filterLocations
    )
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var securityCode = request.SecurityCode;
        var searchLog = new SearchLog
        {
            SearchDate = DateTime.UtcNow,
            SecurityCode = securityCode.Length > 10 ? securityCode[..10] : securityCode,
            DeviceGuid = deviceGuid,
            IsCheckIn = checkType == CheckType.CheckIn,
            EventId = request.EventId,
            IsSearchAllLocations = !filterLocations
        };

        var searchLog2LocationGroups = request.LocationGroups.Select(
            l => new SearchLog2LocationGroup
            {
                LocationGroupId = l,
                SearchLog = searchLog
            });

        var searchLog2Attendances = people.Select(
            p => new SearchLog2Attendance
            {
                AttendanceId = p.AttendanceId,
                SearchLog = searchLog
            });

        await db.AddAsync(searchLog);
        await db.AddRangeAsync(searchLog2LocationGroups);
        await db.AddRangeAsync(searchLog2Attendances);
        await db.SaveChangesAsync();
    }
}