using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Attendee = CheckInsExtension.CheckInUpdateJobs.Models.Attendee;

// ReSharper disable ConvertToUsingDeclaration

namespace ChekInsExtension.Database
{
    public class OverviewRepository : IOverviewRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OverviewRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<ImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocations,
            long eventId, DateTime date)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var people = await (from a in db.Attendances
                        join p in db.People
                            on a.PersonId equals p.Id
                        join at in db.AttendanceTypes
                            on a.AttendanceTypeId equals at.Id
                        where a.InsertDate.Date == date.Date 
                              && selectedLocations.Contains(a.LocationId)
                              && a.EventId == eventId
                        select MapAttendee(a, p, at))
                    .ToListAsync();

                return people.OrderBy(a => a.FirstName).ToImmutableList();
            }
        }

        public async Task<ImmutableList<Attendee>> GetAttendanceHistory(IImmutableList<int> selectedLocations,
            long eventId,
            DateTime startDate,
            DateTime endDate)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var attendees = await (from a in db.Attendances
                        join p in db.People
                            on a.PersonId equals p.Id
                        join at in db.AttendanceTypes
                            on a.AttendanceTypeId equals at.Id
                        where selectedLocations.Contains(a.LocationId)
                              && a.EventId == eventId
                              && a.InsertDate >= startDate.Date
                              && a.InsertDate <= endDate.Date.AddDays(1)
                        select MapAttendee(a, p, at))
                    .ToListAsync();

                return attendees.ToImmutableList();
            }
        }

        private static Attendee MapAttendee(
            Attendance attendance, 
            Person person, 
            AttendanceType attendanceType)
        {
            var checkState = MappingService.GetCheckState(attendance);

            return new Attendee
            {
                CheckInId = attendance.CheckInId,
                FirstName = person.FistName,
                LastName = person.LastName,
                AttendanceType = (AttendanceTypes) attendanceType.Id,
                SecurityCode = attendance.SecurityCode,
                LocationId = attendance.LocationId,
                CheckState = checkState,
                InsertDate = attendance.InsertDate,
                CheckInDate = attendance.CheckInDate,
                CheckOutDate = attendance.CheckOutDate
            };
        }
    }
}