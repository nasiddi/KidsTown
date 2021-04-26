using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.Database;
using KidsTown.IntegrationTests.Mocks;
using KidsTown.KidsTown.Models;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Location = KidsTown.Database.Location;
using Person = KidsTown.Database.Person;

namespace KidsTown.IntegrationTests
{
    public class UpdateTaskTest
    {
        private IServiceProvider _serviceProvider = null!;

        [TearDown]
        public async Task TearDown()
        {
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider).ConfigureAwait(continueOnCapturedContext: false);
        }
        
        [Test]
        public async Task TaskRunsWithoutExceptions()
        {
            _serviceProvider = TestHelper.SetupServiceProviderWithBackgroundTasksDi();
            
            var updateTask = _serviceProvider.GetService<IHostedService>() as UpdateTask;
            await RunTask(updateTask: updateTask!, minExecutionCount: 2).ConfigureAwait(continueOnCapturedContext: false);
            
            Assert.That(actual: updateTask.GetExecutionCount(), expression: Is.GreaterThan(expected: 1));

        }

        [Test]
        public async Task UpdateMockData()
        {
            _serviceProvider = TestHelper.SetupServiceProviderWithBackgroundTasksDiAndMockedPlanningCenterClient();
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider).ConfigureAwait(continueOnCapturedContext: false);
            
            var updateTask = _serviceProvider.GetService<IHostedService>() as UpdateTask;
            await RunTask(updateTask: updateTask!).ConfigureAwait(continueOnCapturedContext: false);

            await AssertUpdateTask().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private static async Task RunTask(UpdateTask updateTask, int minExecutionCount = 1)
        {
            updateTask.ActivateTask();
            
            var task = updateTask.StartAsync(cancellationToken: CancellationToken.None)
                .ConfigureAwait(continueOnCapturedContext: false);

            var watch = Stopwatch.StartNew();
            while (minExecutionCount > updateTask.GetExecutionCount() && watch.ElapsedMilliseconds < 60000)
            {
                await Task.Delay(millisecondsDelay: 100).ConfigureAwait(continueOnCapturedContext: false);
            }
            
            updateTask.DeactivateTask();
            await task;
        }
        
        private async Task<ImmutableList<Data>> GetActualData()
        {
            var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

            await using var db = serviceScopeFactory!.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>();
            var people = await (from a in db.Attendances
                    join p in db.People
                        on a.PersonId equals p.Id
                    join l in db.Locations
                        on a.LocationId equals l.Id
                    where a.CheckInsId < 100
                    select MapData(a, p, l))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return people.ToImmutableList();
        }

        private async Task AssertUpdateTask()
        {
            var expectedData = GetExpectedData();
            var actualData = await GetActualData().ConfigureAwait(continueOnCapturedContext: false);

            expectedData.ForEach(action: e => AssertAttendance(
                expected: e,
                actual: actualData.SingleOrDefault(predicate: a => a.CheckInsId == e.CheckInsId)));

        }

        private static void AssertAttendance(Data expected, Data? actual)
        {
            Assert.That(actual: actual, expression: Is.Not.Null);
            actual.Should().BeEquivalentTo(expectation: expected);
        }

        private static Data MapData(Attendance attendance, Person person, Location location)
        {
            return new(
                firstName: person.FistName,
                lastName: person.LastName,
                checkInsId: attendance.CheckInsId,
                peopleId: person.PeopleId,
                attendanceType: (AttendanceTypes) attendance.AttendanceTypeId,
                testLocation: location.CheckInsLocationId!.Value,
                mayLeaveAlone: person.MayLeaveAlone,
                hasPeopleWithoutPickupPermission: person.HasPeopleWithoutPickupPermission
            );
        }

        private static ImmutableList<Data> GetExpectedData()
        {
            var attendeesData = PlanningCenterClientMock.GetAttendanceData();
            var peopleData = PlanningCenterClientMock.GetPersonData();

            return attendeesData.Select(selector: a =>
            {
                var person = peopleData.SingleOrDefault(predicate: p => p.PeopleId == a.PeopleId);

                return new Data(
                    firstName: person?.FirstName ?? a.FirstName,
                    lastName: person?.LastName ?? a.LastName,
                    checkInsId: a.CheckInsId,
                    peopleId: a.PeopleId,
                    attendanceType: MapAttendanceType(attendeeType: a.AttendanceType),
                    testLocation: (int) a.TestLocation,
                    mayLeaveAlone: person?.MayLeaveAlone ?? true,
                    hasPeopleWithoutPickupPermission: person?.HasPeopleWithoutPickupPermission ?? false);
            }).ToImmutableList();
        }

        private static AttendanceTypes MapAttendanceType(AttendeeType attendeeType)
        {
            return attendeeType switch
            {
                AttendeeType.Guest => AttendanceTypes.Guest,
                AttendeeType.Regular => AttendanceTypes.Regular,
                AttendeeType.Volunteer => AttendanceTypes.Volunteer,
                _ => throw new ArgumentOutOfRangeException(paramName: nameof(attendeeType), actualValue: attendeeType, message: null)
            };
        }

        [SuppressMessage(category: "ReSharper", checkId: "MemberCanBePrivate.Local")]
        [SuppressMessage(category: "ReSharper", checkId: "NotAccessedField.Local")]
        private class Data
        {
            public readonly string FirstName;
            public readonly string LastName;
            public readonly long CheckInsId;
            public readonly long? PeopleId;
            public readonly AttendanceTypes AttendanceType;
            public readonly long TestLocation;
            public readonly bool MayLeaveAlone;
            public readonly bool HasPeopleWithoutPickupPermission;

            public Data(
                string firstName,
                string lastName,
                long checkInsId,
                long? peopleId,
                AttendanceTypes attendanceType,
                long testLocation,
                bool mayLeaveAlone,
                bool hasPeopleWithoutPickupPermission
            )
            {
                FirstName = firstName;
                LastName = lastName;
                CheckInsId = checkInsId;
                PeopleId = peopleId;
                AttendanceType = attendanceType;
                TestLocation = testLocation;
                MayLeaveAlone = mayLeaveAlone;
                HasPeopleWithoutPickupPermission = hasPeopleWithoutPickupPermission;
            }
        }
    }
}