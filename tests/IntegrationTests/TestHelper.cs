using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Database;
using KidsTown.IntegrationTests.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToUsingDeclaration

namespace KidsTown.IntegrationTests
{
    public static class TestHelper
    {
        public static async Task CleanDatabase(IServiceProvider serviceProvider)
        {
            await using (var db = serviceProvider!.GetRequiredService<CheckInsExtensionContext>())
            {
                while (!await db.Database.CanConnectAsync())
                {
                    await Task.Delay(millisecondsDelay: 100);
                }
                
                var attendances = await db.Attendances.Where(predicate: a => a.CheckInId < 100).ToListAsync();
                var people = await db.People.Where(predicate: p => attendances.Select(a => a.PersonId)
                    .Contains(p.Id)).ToListAsync();
                
                db.RemoveRange(entities: attendances);
                db.RemoveRange(entities: people);
                await db.SaveChangesAsync();
            }
        }

        public static async Task InsertTestData(ServiceProvider serviceProvider)
        {
            await using (var db = serviceProvider!.GetRequiredService<CheckInsExtensionContext>())
            {
                while (!await db.Database.CanConnectAsync())
                {
                    await Task.Delay(millisecondsDelay: 100);
                }

                var testData = TestDataFactory.GetTestData();

                var locations = await db.Locations.ToListAsync();

                var people = testData
                    .GroupBy(keySelector: d => d.PeopleId)
                    .Select(selector: d => MapPerson(grouping: d, locations: locations.ToImmutableList()))
                    .ToImmutableList();

                await db.AddRangeAsync(entities: people);
                await db.SaveChangesAsync();
            }
        }

        private static Attendance MapAttendance(TestData.TestData data, ImmutableList<Location> locations)
        {
            var location = locations.Single(predicate: l => l.CheckInsLocationId == (long) data.TestLocation);

            return new Attendance
            {
                CheckInId = data.CheckInId,
                LocationId = location.Id,
                SecurityCode = data.SecurityCode,
                AttendanceTypeId = (int) data.AttendanceType + 1,
                InsertDate = DateTime.UtcNow,
                CheckInDate = null,
                CheckOutDate = null
            };
        }

        private static Person MapPerson(IGrouping<long?, TestData.TestData> grouping, ImmutableList<Location> locations)
        {
            var data = grouping.First();

            var attendances = grouping.Select(selector: g => MapAttendance(data: g, locations: locations));
            
            return new Person
            {
                PeopleId = data.PeopleId,
                FistName = data.PeopleFirstName ?? data.CheckInFirstName,
                LastName = data.PeopleLastName ?? data.CheckInLastName,
                MayLeaveAlone = data.ExpectedMayLeaveAlone ?? true,
                HasPeopleWithoutPickupPermission = data.ExpectedHasPeopleWithoutPickupPermission ?? false,
                Attendances = attendances.ToList()
            };
        }
    }
}