using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Application.Controllers;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.Database;
using KidsTown.IntegrationTests.TestData;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
// ReSharper disable ConvertToUsingDeclaration

namespace KidsTown.IntegrationTests
{
    public class CheckInOutControllerTest
    {
        private ServiceProvider _serviceProvider = null!;

        [TearDown]
        public async Task TearDown()
        {
            await Task.Delay(millisecondsDelay: 1000).ConfigureAwait(continueOnCapturedContext: false);
            await CleanDatabase().ConfigureAwait(continueOnCapturedContext: false);
        }

        [Test]
        public async Task GetPeople_ExistingData_AllFound()
        {
            SetupServiceProvider();
            await CleanDatabase();
            await TestHelper.InsertTestData(serviceProvider: _serviceProvider);

            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var updateTaskMock = new Mock<IUpdateTask>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, updateTask: updateTaskMock.Object);

            var testData = TestDataFactory.GetTestData();

            testData.ForEach(action: async t =>
            {
                await Task.Delay(millisecondsDelay: 100).ConfigureAwait(continueOnCapturedContext: false);
                var request = new CheckInOutRequest
                {
                    SecurityCode = t.SecurityCode,
                    EventId = 389697,
                    SelectedLocationIds = ImmutableList.Create(item: t.LocationGroupId),
                    IsFastCheckInOut = false,
                    CheckType = CheckType.CheckIn,
                    CheckInOutCandidates = ImmutableList<CheckInOutCandidate>.Empty
                };
                
                var actionResult = await controller.GetPeople(request: request);
                var okResult = actionResult as OkObjectResult;
                var checkInOutResult = okResult!.Value as CheckInOutResult;
                
                var candidates = checkInOutResult!.CheckInOutCandidates.Select(selector: c => c.Name).ToArray<object>();
                Assert.That(actual: $"{t.PeopleFirstName ?? t.CheckInsFirstName} {t.PeopleLastName ?? t.CheckInsLastName}", 
                    expression: Is.AnyOf(expected: candidates));
            });
        }

        [Test]
        public async Task GetPeople_NoLocationGroupSet_NothingFound()
        {
            SetupServiceProvider();
            await CleanDatabase();
            await TestHelper.InsertTestData(serviceProvider: _serviceProvider);

            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var updateTaskMock = new Mock<IUpdateTask>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, updateTask: updateTaskMock.Object);

            var testData = TestDataFactory.GetTestData();
            
            testData.ForEach(action: async t =>
            {
                var request = new CheckInOutRequest
                {
                    SecurityCode = t.SecurityCode,
                    EventId = 389697,
                    SelectedLocationIds = ImmutableList<int>.Empty,
                    IsFastCheckInOut = false,
                    CheckType = CheckType.CheckIn,
                    CheckInOutCandidates = ImmutableList<CheckInOutCandidate>.Empty
                };
                
                var actionResult = await controller.GetPeople(request: request);
                var okResult = actionResult as OkObjectResult;
                var checkInOutResult = okResult!.Value as CheckInOutResult;
                
                Assert.That(actual: checkInOutResult?.AlertLevel, expression: Is.EqualTo(expected: AlertLevel.Danger));
            });
        }
        
        [Test]
        
        public async Task GetPeople_FastCheckInActive_CheckedIn()
        {
            SetupServiceProvider();
            await CleanDatabase();
            
            var testData = TestDataFactory.GetTestData().GroupBy(keySelector: t => t.SecurityCode).ToImmutableList();
            var filteredTestData = testData.Where(predicate: t => t.Count() == 1)
                .SelectMany(selector: g => g.ToImmutableList())
                .Where(predicate: a => a.PeopleId == 1)
                .ToImmutableList();
            
            await TestHelper.InsertTestData(serviceProvider: _serviceProvider, testData: filteredTestData);
            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var updateTaskMock = new Mock<IUpdateTask>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, updateTask: updateTaskMock.Object);

            filteredTestData.ForEach(action: async t =>
            {
                var request = new CheckInOutRequest
                {
                    SecurityCode = t.SecurityCode,
                    EventId = 389697,
                    SelectedLocationIds = ImmutableList.Create(item: t.LocationGroupId),
                    IsFastCheckInOut = true,
                    CheckType = CheckType.CheckIn,
                    CheckInOutCandidates = ImmutableList<CheckInOutCandidate>.Empty
                };
                
                var actionResult = await controller.GetPeople(request: request);
                var okResult = actionResult as OkObjectResult;
                var checkInOutResult = okResult!.Value as CheckInOutResult;
                
                Assert.That(actual: checkInOutResult?.AlertLevel, expression: Is.EqualTo(expected: AlertLevel.Success));
                Assert.That(actual: checkInOutResult?.SuccessfulFastCheckout, expression: Is.True);
            });

            var actualData = await GetActualData(checkInsIds: filteredTestData.Select(selector: t => t.CheckInsId).ToImmutableList());
            Assert.That(actual: actualData.Count(predicate: a => a.CheckInDate == null), expression: Is.Zero);
            Assert.That(actual: actualData.Count, expression: Is.EqualTo(expected: filteredTestData.Count));
        }
        
        private void SetupServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json", optional: false)
                .AddJsonFile(path: "appsettings.Secrets.json", optional: false)
                .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
                .Build();

            services.AddSingleton<IConfiguration>(implementationFactory: _ => configuration);
            
            services.AddScoped<ICheckInOutService, CheckInOutService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IOverviewService, OverviewService>();
            
            services.AddScoped<ICheckInOutRepository, CheckInOutRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IOverviewRepository, OverviewRepository>();

            services.AddDbContext<KidsTownContext>(
                contextLifetime: ServiceLifetime.Transient,
                optionsAction: o
                    => o.UseSqlServer(connectionString: configuration.GetConnectionString(name: "Database")));
            
            _serviceProvider =  services.BuildServiceProvider();
        }
        
        private async Task<ImmutableList<Data>> GetActualData(ImmutableList<long> checkInsIds)
        {
            await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
            
            var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();
            
            await using (var db = serviceScopeFactory!.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>())
            {
                var attendances = await (from a in db.Attendances
                        where a.CheckInsId < 100 && checkInsIds.Contains(a.CheckInsId)
                        select MapData(a))
                    .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

                return attendances.ToImmutableList();
            }   
        }

        private static Data MapData(Attendance attendance) => new(checkInDate: attendance.CheckInDate);

        private class Data
        {
            public readonly DateTime? CheckInDate;

            public Data(DateTime? checkInDate)
            {
                CheckInDate = checkInDate;
            }
        }
        
        private async Task CleanDatabase()
        {
            await TestHelper.CleanDatabase(serviceProvider: _serviceProvider);
        }
    }
}