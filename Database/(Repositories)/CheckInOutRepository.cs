using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable ConvertToUsingDeclaration

namespace ChekInsExtension.Database
{
    public class CheckInOutRepository : ICheckInOutRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CheckInOutRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Person>> GetPeople(
            PeopleSearchParameters peopleSearchParameters)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var people = await (from a in db.Attendances
                    join p in db.People
                        on a.PersonId equals p.Id
                    join l in db.Locations
                        on a.LocationId equals l.Id
                    where a.SecurityCode == peopleSearchParameters.SecurityCode
                          && peopleSearchParameters.LocationGroups.Contains(l.LocationGroupId)
                          && a.InsertDate >= DateTime.Today.AddDays(-3)
                          && l.EventId == peopleSearchParameters.EventId
                    select MapPerson(a, p, l))
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return people.ToImmutableList();
            }
        }

        public async Task<bool> CheckInPeople(IImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkinIds: checkInIds, db: db).ConfigureAwait(continueOnCapturedContext: false);
                checkIns.ForEach(action: c => c.CheckInDate = DateTime.UtcNow);
                var result = await db.SaveChangesAsync();
                return result > 0;            }
        }
        
        public async Task<bool> CheckOutPeople(IImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkinIds: checkInIds, db: db).ConfigureAwait(continueOnCapturedContext: false);
                checkIns.ForEach(action: c => c.CheckOutDate = DateTime.UtcNow);
                var result = await db.SaveChangesAsync();
                return result > 0;
            }
        }

        public async Task<bool> SetCheckState(CheckState revertedCheckState, ImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkinIds: checkInIds, db: db).ConfigureAwait(continueOnCapturedContext: false);

                switch (revertedCheckState)
                {
                    case CheckState.PreCheckedIn:
                        checkIns.ForEach(action: c => c.CheckInDate = null);
                        break;
                    case CheckState.CheckedIn:
                        checkIns.ForEach(action: c => c.CheckOutDate = null);
                        break;
                    case CheckState.CheckedOut:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(paramName: nameof(revertedCheckState), actualValue: revertedCheckState, message: null);
                }

                var result = await db.SaveChangesAsync();
                return result > 0;
            }
        }

        private static async Task<List<Attendance>> GetCheckIns(
            IImmutableList<int> checkinIds,
            CheckInsExtensionContext db)
        {
            var checkIns = await db.Attendances.Where(predicate: a => 
                checkinIds.Contains(a.Id)).ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            return checkIns;
        }

        private static CheckInsExtension.CheckInUpdateJobs.Models.Person MapPerson(Attendance attendance, Person person,
            Location location)
        {
            var checkState = MappingService.GetCheckState(attendance: attendance);

            return new CheckInsExtension.CheckInUpdateJobs.Models.Person
            {
                CheckInId = attendance.Id,
                SecurityCode = attendance.SecurityCode,
                Location = location.Name,
                FirstName = person.FistName,
                LastName = person.LastName,
                CheckInTime = attendance.CheckInDate,
                CheckOutTime = attendance.CheckOutDate,
                MayLeaveAlone = person.MayLeaveAlone,
                HasPeopleWithoutPickupPermission = person.HasPeopleWithoutPickupPermission,
                CheckState = checkState
            };
        }
    }
}