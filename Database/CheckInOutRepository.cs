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
        
        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Person>> GetPeople(
            PeopleSearchParameters peopleSearchParameters, 
            IImmutableList<long> eventIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var people = await (from a in db.Attendances
                    join p in db.People
                        on a.PersonId equals p.Id
                    join l in db.Locations
                        on a.LocationId equals l.Id
                    where a.SecurityCode == peopleSearchParameters.SecurityCode
                          && peopleSearchParameters.Locations.Contains(a.Location.Id)
                          && a.InsertDate >= DateTime.Today.AddDays(-30)
                          && eventIds.Contains(a.EventId)
                    select MapPerson(a, p, l))
                    .ToListAsync();

                return people.ToImmutableList();
            }
        }

        public async Task<bool> CheckInPeople(IImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkInIds, db);
                checkIns.ForEach(c => c.CheckInDate = DateTime.UtcNow);
                var result = await db.SaveChangesAsync();
                return result > 0;            }
        }
        
        public async Task<bool> CheckOutPeople(IImmutableList<int> checkInIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var checkIns = await GetCheckIns(checkInIds, db);
                checkIns.ForEach(c => c.CheckOutDate = DateTime.UtcNow);
                var result = await db.SaveChangesAsync();
                return result > 0;
            }
        }

        public async Task<ImmutableList<CheckInsExtension.CheckInUpdateJobs.Models.Location>> GetActiveLocations()
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var locations = await db.Locations.Where(l => l.IsEnabled).ToListAsync();
                return locations.Select(l => new CheckInsExtension.CheckInUpdateJobs.Models.Location(l.Id, l.Name))
                    .ToImmutableList();
            }
        }
        
        public async Task<ImmutableList<Attendee>> GetActiveAttendees(
            IImmutableList<int> selectedLocations,
            IImmutableList<long> eventIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var people = await (from a in db.Attendances
                        join p in db.People
                            on a.PersonId equals p.Id
                        join l in db.Locations
                            on a.LocationId equals l.Id
                        join at in db.AttendanceTypes
                            on a.AttendanceTypeId equals at.Id
                        where a.CheckInDate != null 
                              && a.CheckOutDate == null
                              && selectedLocations.Contains(l.Id)
                              && eventIds.Contains(a.EventId)
                        select MapAttendee(a, p, l, at))
                    .ToListAsync();

                return people.ToImmutableList();
            }
        }

        private static Attendee MapAttendee(
            Attendance attendance, 
            Person person, 
            Location location,
            AttendanceType attendanceType)
        {
            var checkState = GetCheckState(attendance);

            return new Attendee
            {
                Name = $"{person.FistName} {person.LastName}",
                AttendanceType = attendanceType.Name,
                LocationId = location.Id,
                CheckState = checkState
            };
        }

        private static async Task<List<Attendance>> GetCheckIns(
            IImmutableList<int> checkinIds,
            CheckInsExtensionContext db)
        {
            var checkIns = await db.Attendances.Where(a => 
                checkinIds.Contains(a.Id)).ToListAsync();
            return checkIns;
        }

        private static CheckInsExtension.CheckInUpdateJobs.Models.Person MapPerson(Attendance attendance, Person person,
            Location location)
        {
            var checkState = GetCheckState(attendance);

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

        private static CheckState GetCheckState(Attendance attendance)
        {
            var checkState = CheckState.PreCheckedIn;

            if (attendance.CheckInDate.HasValue)
            {
                checkState = CheckState.CheckedIn;
            }

            if (attendance.CheckOutDate.HasValue)
            {
                checkState = CheckState.CheckedOut;
            }

            return checkState;
        }
    }
}