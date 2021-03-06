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

        public async Task<ImmutableList<Attendee>> GetAttendanceHistory(IImmutableList<int> selectedLocations, IImmutableList<long> eventIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CheckInsExtensionContext>())
            {
                var attendees = await (from a in db.Attendances
                        join p in db.People
                            on a.PersonId equals p.Id
                        join l in db.Locations
                            on a.LocationId equals l.Id
                        join at in db.AttendanceTypes
                            on a.AttendanceTypeId equals at.Id
                        where selectedLocations.Contains(l.Id)
                              && eventIds.Contains(a.EventId)
                        select MapAttendee(a, p, l, at))
                    .ToListAsync();

                return attendees.ToImmutableList();
            }
        }

        private static Attendee MapAttendee(
            Attendance attendance, 
            Person person, 
            Location location,
            AttendanceType attendanceType)
        {
            var checkState = MappingService.GetCheckState(attendance);

            return new Attendee(
                $"{person.FistName} {person.LastName}",
                (AttendanceTypes) attendanceType.Id,
                location.Id,
                checkState,
                attendance.InsertDate,
                attendance.CheckInDate,
                attendance.CheckOutDate);
        }
    }
}