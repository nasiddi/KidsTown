using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChekInsExtension.Database
{
    public class CheckInOutRepository : ICheckInOutRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CheckInOutRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        //Todo add date filter
        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Person>> GetPeople(
            PeopleSearchParameters peopleSearchParameters)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInExtensionContext>())
            {
                var people = await (from c in db.CheckIns
                    join p in db.People
                        on c.PersonId equals p.Id
                    join l in db.Locations
                        on c.LocationId equals l.Id
                    where c.SecurityCode == peopleSearchParameters.SecurityCode 
                          && peopleSearchParameters.Locations.Contains(c.Location.Id)
                    select MapPerson(c, p, l))
                    .ToListAsync();

                return people.ToImmutableList();
            }
        }

        public async Task<bool> CheckInPeople(IImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkInIds, db);
                checkIns.ForEach(c => c.CheckInDate = DateTime.UtcNow);
                var result = await db.SaveChangesAsync();
                return result > 0;            }
        }
        
        public async Task<bool> CheckOutPeople(IImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkInIds, db);
                checkIns.ForEach(c => c.CheckOutDate = DateTime.UtcNow);
                var result = await db.SaveChangesAsync();
                return result > 0;
            }
        }

        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Location>> GetActiveLocations()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInExtensionContext>())
            {
                var locations = await db.Locations.Where(l => l.IsEnabled).ToListAsync();
                return locations.Select(l => new CheckInsExtension.CheckInUpdateJobs.Models.Location(l.Id, l.Name))
                    .ToImmutableList();
            }
        }

        private static async Task<List<CheckIn>> GetCheckIns(
            IImmutableList<int> checkinIds,
            CheckInExtensionContext db)
        {
            var checkIns = await db.CheckIns.Where(c => 
                checkinIds.Contains(c.Id)).ToListAsync();
            return checkIns;
        }

        private static CheckInsExtension.CheckInUpdateJobs.Models.Person MapPerson(CheckIn checkIn, Person person,
            Location location)
        {
            var checkState = CheckState.PreCheckedIn;

            if (checkIn.CheckInDate.HasValue)
            {
                checkState = CheckState.CheckedIn;
            }

            if (checkIn.CheckOutDate.HasValue)
            {
                checkState = CheckState.CheckedOut;
            }
            
            
            return new()
            {
                CheckInId = checkIn.Id,
                SecurityCode = checkIn.SecurityCode,
                Location = location.Name,
                FirstName = person.FistName,
                LastName = person.LastName,
                CheckInTime = checkIn.CheckInDate,
                CheckOutTime = checkIn.CheckOutDate,
                MayLeaveAlone = person.MayLeaveAlone ?? true,
                HasPeopleWithoutPickupPermission = person.HasPeopleWithoutPickupPermission ?? false,
                CheckState = checkState
            };
        }
    }
}