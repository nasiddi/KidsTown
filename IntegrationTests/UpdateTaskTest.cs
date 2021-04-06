using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Models;
using CheckInsExtension.CheckInUpdateJobs.Update;
using CheckInsExtension.PlanningCenterAPIClient;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using ChekInsExtension.Database;
using FluentAssertions;
using IntegrationTests.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Location = ChekInsExtension.Database.Location;
using Person = ChekInsExtension.Database.Person;

// ReSharper disable ConvertToUsingDeclaration

namespace IntegrationTests
{
    public class UpdateTaskTest
    {
        private IServiceProvider _serviceProvider = null!;

        [TearDown]
        public async Task TearDown()
        {
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider);
        }
        
        [Test]
        public async Task TaskRunsWithoutExceptions()
        {
            SetupServiceProvider();
            
            var updateTask = _serviceProvider.GetService<IHostedService>() as UpdateTask;
            await RunTask(updateTask: updateTask!, minExecutionCount: 2);
            
            Assert.That(actual: updateTask.GetExecutionCount(), expression: Is.GreaterThan(expected: 1));

        }

        [Test]
        public async Task UpdateMockData()
        {
            SetupServiceProvider(mockPlanningCenterClient: true);
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider);
            
            var updateTask = _serviceProvider.GetService<IHostedService>() as UpdateTask;
            await RunTask(updateTask: updateTask!);

            await AssertUpdateTask();
        }
        
        private static async Task RunTask(UpdateTask updateTask, int minExecutionCount = 1)
        {
            updateTask.ActivateTask();
            
            var task = updateTask.StartAsync(cancellationToken: CancellationToken.None)
                .ConfigureAwait(continueOnCapturedContext: false);

            var watch = Stopwatch.StartNew();
            while (minExecutionCount > updateTask.GetExecutionCount() && watch.ElapsedMilliseconds < 60000)
            {
                await Task.Delay(millisecondsDelay: 100);
            }
            
            updateTask.DeactivateTask();
            await task;
        }
        
        private void SetupServiceProvider(bool mockPlanningCenterClient = false)
        {
            IServiceCollection services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json", optional: false)
                .AddJsonFile(path: "appsettings.Secrets.json", optional: false)
                .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
                .Build();

            services.AddSingleton<IConfiguration>(implementationFactory: _ => configuration);
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddHostedService<UpdateTask>();

            if (mockPlanningCenterClient)
            {
                services.AddSingleton<IPlanningCenterClient, PlanningCenterClientMock>();
            }
            else
            {
                services.AddSingleton<IPlanningCenterClient, PlanningCenterClient>();
            }
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<IUpdateRepository, UpdateRepository>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(serviceType: typeof(ILogger), implementationType: typeof(Logger<UpdateTask>));
            services.AddDbContext<CheckInsExtensionContext>(
                contextLifetime: ServiceLifetime.Transient,
                optionsAction: o
                => o.UseSqlServer(connectionString: configuration.GetConnectionString(name: "Database")));
            
            _serviceProvider =  services.BuildServiceProvider();
        }

        private async Task<ImmutableList<Data>> GetActualData()
        {
            var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();
            
            await using (var db = serviceScopeFactory!.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var people = await (from a in db.Attendances
                        join p in db.People
                            on a.PersonId equals p.Id
                        join l in db.Locations
                            on a.LocationId equals l.Id
                        where a.CheckInId < 100
                        select MapData(a, p, l))
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return people.ToImmutableList();
            }   
        }

        private async Task AssertUpdateTask()
        {
            var expectedData = GetExpectedData();
            var actualData = await GetActualData();

            expectedData.ForEach(action: e => AssertAttendance(
                expected: e,
                actual: actualData.SingleOrDefault(predicate: a => a.CheckInId == e.CheckInId)));

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
                checkInId: attendance.CheckInId,
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
                    checkInId: a.CheckInId,
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
            public readonly long CheckInId;
            public readonly long? PeopleId;
            public readonly AttendanceTypes AttendanceType;
            public readonly long TestLocation;
            public readonly bool MayLeaveAlone;
            public readonly bool HasPeopleWithoutPickupPermission;

            public Data(
                string firstName,
                string lastName,
                long checkInId,
                long? peopleId,
                AttendanceTypes attendanceType,
                long testLocation,
                bool mayLeaveAlone,
                bool hasPeopleWithoutPickupPermission
            )
            {
                FirstName = firstName;
                LastName = lastName;
                CheckInId = checkInId;
                PeopleId = peopleId;
                AttendanceType = attendanceType;
                TestLocation = testLocation;
                MayLeaveAlone = mayLeaveAlone;
                HasPeopleWithoutPickupPermission = hasPeopleWithoutPickupPermission;
            }
        }
    }
}