using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KidsTown.Shared;

namespace KidsTown.Database
{
    public class CheckInOutRepository : ICheckInOutRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CheckInOutRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public async Task<IImmutableList<KidsTown.Models.Kid>> GetPeople(
            PeopleSearchParameters peopleSearchParameters)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var people = await (from a in db.Attendances
                    join p in db.People
                        on a.PersonId equals p.Id
                    join k in db.Kids.DefaultIfEmpty()
                        on p.Id equals k.PersonId
                    join l in db.Locations
                        on a.LocationId equals l.Id
                    where a.SecurityCode == peopleSearchParameters.SecurityCode
                          && peopleSearchParameters.LocationGroups.Contains(l.LocationGroupId)
                          && a.InsertDate >= DateTime.Today.AddDays(-3)
                          && l.EventId == peopleSearchParameters.EventId
                    select MapKid(a, p, k, l))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return people.ToImmutableList();
        }

        public async Task<bool> CheckInPeople(IImmutableList<int> attendanceIds)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var attendances = await GetAttendances(attendanceIds: attendanceIds, db: db).ConfigureAwait(continueOnCapturedContext: false);
            attendances.ForEach(action: a => a.CheckInDate = DateTime.UtcNow);
            var result = await db.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> CheckOutPeople(IImmutableList<int> attendanceIds)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var attendances = await GetAttendances(attendanceIds: attendanceIds, db: db).ConfigureAwait(continueOnCapturedContext: false);
            attendances.ForEach(action: a => a.CheckOutDate = DateTime.UtcNow);
            var result = await db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> SetCheckState(CheckState revertedCheckState, IImmutableList<int> attendanceIds)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var attendances = await GetAttendances(attendanceIds: attendanceIds, db: db).ConfigureAwait(continueOnCapturedContext: false);

            switch (revertedCheckState)
            {
                case CheckState.None:
                    var people = await db.People.Where(predicate: p => attendances.Select(a => a.PersonId).Contains(p.Id)).ToListAsync();
                    var kids = await db.Kids.Where(predicate: k => people.Select(p => p.Id).Contains(k.PersonId)).ToListAsync();
                    db.RemoveRange(entities: attendances);
                    db.RemoveRange(entities: kids);
                    db.RemoveRange(entities: people);
                    break;
                case CheckState.PreCheckedIn:
                    attendances.ForEach(action: c => c.CheckInDate = null);
                    break;
                case CheckState.CheckedIn:
                    attendances.ForEach(action: c => c.CheckOutDate = null);
                    break;
                case CheckState.CheckedOut:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(revertedCheckState), actualValue: revertedCheckState, message: null);
            }

            var result = await db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<int> CreateGuest(int locationId, string securityCode, string firstName, string lastName)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var kid = new Kid
            {
                MayLeaveAlone = true,
                HasPeopleWithoutPickupPermission = false
            };

            var person = new Person
            {
                FirstName = firstName,
                LastName = lastName,
                UpdateDate = DateTime.UtcNow,
                Kid = kid
            };
                
            var attendance = new Attendance
            {
                CheckInsId = 0,
                LocationId = locationId,
                SecurityCode = securityCode, 
                AttendanceTypeId = (int) AttendanceTypeId.Guest,
                InsertDate = DateTime.UtcNow,
                Person = person
            };

            var entry = await db.AddAsync(entity: attendance);
            await db.SaveChangesAsync();
            return entry.Entity.Id;
        }

        public async Task<bool> SecurityCodeExists(string securityCode)
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);
            
            var attendanceCount = await db.Attendances
                .Where(predicate: a => a.SecurityCode == securityCode
                                       && a. InsertDate > DateTime.Today).CountAsync();
            return attendanceCount > 0;
        }

        private static async Task<List<Attendance>> GetAttendances(
            IImmutableList<int> attendanceIds,
            KidsTownContext db)
        {
            var attendances = await db.Attendances.Where(predicate: a => 
                attendanceIds.Contains(a.Id)).ToListAsync().ConfigureAwait(continueOnCapturedContext: false);
            return attendances;
        }

        private static KidsTown.Models.Kid MapKid(
            Attendance attendance,
            Person person,
            Kid? kid,
            Location location
        )
        {
            var checkState = MappingService.GetCheckState(attendance: attendance);

            return new KidsTown.Models.Kid
            {
                AttendanceId = attendance.Id,
                SecurityCode = attendance.SecurityCode,
                Location = location.Name,
                FirstName = person.FirstName,
                LastName = person.LastName,
                CheckInTime = attendance.CheckInDate,
                CheckOutTime = attendance.CheckOutDate,
                MayLeaveAlone = kid?.MayLeaveAlone ?? true,
                HasPeopleWithoutPickupPermission = kid?.HasPeopleWithoutPickupPermission ?? false,
                CheckState = checkState
            };
        }
    }
}
