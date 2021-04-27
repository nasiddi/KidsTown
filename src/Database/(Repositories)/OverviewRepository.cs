using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Attendee = KidsTown.KidsTown.Models.Attendee;

// ReSharper disable ConvertToUsingDeclaration

namespace KidsTown.Database
{
    public class OverviewRepository : IOverviewRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OverviewRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<ImmutableList<Attendee>> GetActiveAttendees(IImmutableList<int> selectedLocationGroups,
            long eventId, DateTime date)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>())
            {
                var people = await (from a in db.Attendances
                        join p in db.Kids
                            on a.KidId equals p.Id
                        join at in db.AttendanceTypes
                            on a.AttendanceTypeId equals at.Id
                        join l in db.Locations
                            on a.LocationId equals l.Id
                        where a.InsertDate.Date == date.Date 
                              && selectedLocationGroups.Contains(l.LocationGroupId)
                              && l.EventId == eventId
                        select MapAttendee(a, p, at, l))
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return people.OrderBy(keySelector: a => a.FirstName).ToImmutableList();
            }
        }

        public async Task<ImmutableList<Attendee>> GetAttendanceHistoryByLocations(
            long eventId,
            DateTime startDate,
            DateTime endDate,
            IImmutableList<int> selectedLocations
        )
        {
            return await GetAttendanceHistory(
                eventId: eventId,
                startDate: startDate,
                endDate: endDate,
                selectedLocations: selectedLocations)
                .ConfigureAwait(continueOnCapturedContext: false);   
        }

        public async Task<ImmutableList<Attendee>> GetAttendanceHistoryByLocationGroups(
            long eventId,
            DateTime startDate,
            DateTime endDate,
            IImmutableList<int> selectedLocationGroups
        )
        {
            return await GetAttendanceHistory(
                eventId: eventId,
                startDate: startDate,
                endDate: endDate,
                selectedLocationGroups: selectedLocationGroups)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task<ImmutableList<Attendee>> GetAttendanceHistory(
            long eventId,
            DateTime startDate,
            DateTime endDate,
            IImmutableList<int> selectedLocations = null!,
            IImmutableList<int> selectedLocationGroups = null!)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>())
            {
                var attendees = await (from a in db.Attendances
                        join p in db.Kids
                            on a.KidId equals p.Id
                        join at in db.AttendanceTypes
                            on a.AttendanceTypeId equals at.Id
                        join l in db.Locations
                            on a.LocationId equals l.Id
                        where (selectedLocations != null && selectedLocations.Contains(l.Id) 
                               || selectedLocationGroups != null && selectedLocationGroups.Contains(l.LocationGroupId))
                              && l.EventId == eventId
                              && a.InsertDate >= startDate.Date
                              && a.InsertDate <= endDate.Date.AddDays(1)
                        select MapAttendee(a, p, at, l))
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return attendees.ToImmutableList();
            }
        }

        private static Attendee MapAttendee(
            Attendance attendance, 
            Kid kid, 
            AttendanceType attendanceType,
            Location location)
        {
            var checkState = MappingService.GetCheckState(attendance: attendance);

            return new Attendee
            {
                AttendanceId = attendance.Id,
                FirstName = kid.FistName,
                LastName = kid.LastName,
                AttendanceType = (AttendanceTypes) attendanceType.Id,
                SecurityCode = attendance.SecurityCode,
                LocationGroupId = location.LocationGroupId,
                Location = location.Name,
                CheckState = checkState,
                InsertDate = attendance.InsertDate,
                CheckInDate = attendance.CheckInDate,
                CheckOutDate = attendance.CheckOutDate
            };
        }
    }
}