using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KidsTown.BackgroundTasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Kid;
using KidsTown.Database;
using KidsTown.IntegrationTests.Mocks;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

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
            
            var updateTask = _serviceProvider.GetService<AttendanceUpdateTask>();
            await RunTask(backgroundTask: updateTask!, minExecutionCount: 2).ConfigureAwait(continueOnCapturedContext: false);
            
            Assert.That(actual: updateTask.GetExecutionCount(), expression: Is.GreaterThan(expected: 1));

        }

        [Test]
        public async Task UpdateMockData()
        {
            _serviceProvider = TestHelper.SetupServiceProviderWithBackgroundTasksDiAndMockedPlanningCenterClient();
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider).ConfigureAwait(continueOnCapturedContext: false);
            
            var attendanceUpdateTask = _serviceProvider.GetService<AttendanceUpdateTask>();
            await RunTask(backgroundTask: attendanceUpdateTask!).ConfigureAwait(continueOnCapturedContext: false);

            var kidUpdateTask = _serviceProvider.GetService<KidUpdateTask>();
            await RunTask(backgroundTask: kidUpdateTask!).ConfigureAwait(continueOnCapturedContext: false);

            var adultUpdateTask = _serviceProvider.GetService<AdultUpdateTask>();
            await RunTask(backgroundTask: adultUpdateTask!).ConfigureAwait(continueOnCapturedContext: false);

            await AssertUpdateTask().ConfigureAwait(continueOnCapturedContext: false);
        }
        
        private static async Task RunTask(BackgroundTask backgroundTask, int minExecutionCount = 1)
        {
            backgroundTask.ActivateTask();
            
            var task = backgroundTask.StartAsync(cancellationToken: CancellationToken.None)
                .ConfigureAwait(continueOnCapturedContext: false);

            var watch = Stopwatch.StartNew();
            while (minExecutionCount > backgroundTask.GetExecutionCount() && watch.ElapsedMilliseconds < 60000)
            {
                await Task.Delay(millisecondsDelay: 100).ConfigureAwait(continueOnCapturedContext: false);
            }
            
            backgroundTask.DeactivateTask();
            await task;
        }
        
        private async Task<IImmutableList<Data>> GetActualData()
        {
            var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

            await using var db = serviceScopeFactory!.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>();

            var people = await db.Attendances
                .Include(navigationPropertyPath: i => i.Person)
                .Include(navigationPropertyPath: i => i.Person.Kid)
                .Include(navigationPropertyPath: i => i.Location)
                .Where(predicate: p => p.CheckInsId < 100)
                .Select(selector: p => MapData(p))
                .ToListAsync();
            
            return people.ToImmutableList();
        }

        private async Task AssertUpdateTask()
        {
            var expectedData = GetExpectedData();
            var actualData = await GetActualData().ConfigureAwait(continueOnCapturedContext: false);

            (expectedData as ImmutableList<Data>)?.ForEach(action: e => AssertAttendance(
                expected: e,
                actual: actualData.SingleOrDefault(predicate: a => a.CheckInsId == e.CheckInsId)));

        }

        private static void AssertAttendance(Data expected, Data? actual)
        {
            Assert.That(actual: actual, expression: Is.Not.Null);
            actual.Should().BeEquivalentTo(expectation: expected);
        }

        private static Data MapData(Attendance attendance)
        {
            return new(
                firstName: attendance.Person.FirstName,
                lastName: attendance.Person.LastName,
                checkInsId: attendance.CheckInsId,
                peopleId: attendance.Person.PeopleId,
                attendanceTypeId: (AttendanceTypeId) attendance.AttendanceTypeId,
                testLocation: attendance.Location.CheckInsLocationId!.Value,
                mayLeaveAlone: attendance.Person.Kid?.MayLeaveAlone ?? true,
                hasPeopleWithoutPickupPermission: attendance.Person.Kid?.HasPeopleWithoutPickupPermission ?? false
            );
        }

        private static IImmutableList<Data> GetExpectedData()
        {
            var attendeesData = PlanningCenterClientMock.GetAttendanceData();
            var peopleData = PlanningCenterClientMock.GetKidsData();

            return attendeesData.Select(selector: a =>
            {
                var kid = peopleData.SingleOrDefault(predicate: p => p.PeopleId == a.PeopleId);

                return new Data(
                    firstName: kid?.FirstName ?? a.FirstName,
                    lastName: kid?.LastName ?? a.LastName,
                    checkInsId: a.CheckInsId,
                    peopleId: a.PeopleId,
                    attendanceTypeId: MapAttendanceType(attendeeType: a.AttendanceType),
                    testLocation: (int) a.TestLocation,
                    mayLeaveAlone: kid?.MayLeaveAlone ?? true,
                    hasPeopleWithoutPickupPermission: kid?.HasPeopleWithoutPickupPermission ?? false);
            }).ToImmutableList();
        }

        private static AttendanceTypeId MapAttendanceType(AttendeeType attendeeType)
        {
            return attendeeType switch
            {
                AttendeeType.Guest => AttendanceTypeId.Guest,
                AttendeeType.Regular => AttendanceTypeId.Regular,
                AttendeeType.Volunteer => AttendanceTypeId.Volunteer,
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
            public readonly AttendanceTypeId AttendanceTypeId;
            public readonly long TestLocation;
            public readonly bool MayLeaveAlone;
            public readonly bool HasPeopleWithoutPickupPermission;

            public Data(
                string firstName,
                string lastName,
                long checkInsId,
                long? peopleId,
                AttendanceTypeId attendanceTypeId,
                long testLocation,
                bool mayLeaveAlone,
                bool hasPeopleWithoutPickupPermission
            )
            {
                FirstName = firstName;
                LastName = lastName;
                CheckInsId = checkInsId;
                PeopleId = peopleId;
                AttendanceTypeId = attendanceTypeId;
                TestLocation = testLocation;
                MayLeaveAlone = mayLeaveAlone;
                HasPeopleWithoutPickupPermission = hasPeopleWithoutPickupPermission;
            }
        }
    }
}