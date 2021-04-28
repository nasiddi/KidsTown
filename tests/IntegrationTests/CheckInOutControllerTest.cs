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
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

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
            // Arrange
            var controller = await SetupTestEnvironment().ConfigureAwait(continueOnCapturedContext: false);
            var testData = TestDataFactory.GetTestData();

            // Act & Assert
            (testData as ImmutableList<TestData.TestData>)?.ForEach(action: async t =>
            {
                var checkInOutResult = await SendRequest(
                    securityCode: t.SecurityCode,
                    selectedLocationIds: ImmutableList.Create(item: t.LocationGroupId),
                    isFastCheckInOut: false,
                    controller: controller)
                    .ConfigureAwait(continueOnCapturedContext: false);
                
                var candidates = checkInOutResult!.CheckInOutCandidates.Select(selector: c => c.Name).ToArray<object>();
                Assert.That(actual: $"{t.PeopleFirstName ?? t.CheckInsFirstName} {t.PeopleLastName ?? t.CheckInsLastName}", 
                    expression: Is.AnyOf(expected: candidates));
            });
        }

        [Test]
        public async Task GetPeople_NoLocationGroupSet_NothingFound()
        {
            // Arrange
            var controller = await SetupTestEnvironment().ConfigureAwait(continueOnCapturedContext: false);
            var testData = TestDataFactory.GetTestData();
            
            // Act & Assert
            (testData as ImmutableList<TestData.TestData>)?.ForEach(action: async t =>
            {
                var checkInOutResult = await SendRequest(
                    securityCode: t.SecurityCode,
                    selectedLocationIds: ImmutableList<int>.Empty,
                    isFastCheckInOut: false,
                    controller: controller)
                    .ConfigureAwait(continueOnCapturedContext: false);

                Assert.That(actual: checkInOutResult?.AlertLevel, expression: Is.EqualTo(expected: AlertLevel.Danger));
            });
        }

        [Test]
        public async Task GetPeople_FastCheckInActive_CheckedIn()
        {
            // Arrange
            _serviceProvider = TestHelper.SetupServiceProviderWithKidsTownDi();
            await CleanDatabase().ConfigureAwait(continueOnCapturedContext: false);
            
            var testData = TestDataFactory.GetTestData().GroupBy(keySelector: t => t.SecurityCode).ToImmutableList();
            var filteredTestData = testData.Where(predicate: t => t.Count() == 1)
                .SelectMany(selector: g => g.ToImmutableList())
                .Where(predicate: a => a.PeopleId == 1)
                .ToImmutableList();
            
            await TestHelper.InsertTestData(serviceProvider: _serviceProvider, testData: filteredTestData);
            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var updateTaskMock = new Mock<IUpdateTask>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, updateTask: updateTaskMock.Object);
            
            // Act
            filteredTestData.ForEach(action: async t =>
            {
                var checkInOutResult = await SendRequest(
                    securityCode: t.SecurityCode,
                    selectedLocationIds: ImmutableList.Create(item: t.LocationGroupId),
                    isFastCheckInOut: true,
                    controller: controller)
                    .ConfigureAwait(continueOnCapturedContext: false);

                Assert.That(actual: checkInOutResult.AlertLevel, expression: Is.EqualTo(expected: AlertLevel.Success));
                Assert.That(actual: checkInOutResult.SuccessfulFastCheckout, expression: Is.True);
            });

            // Assert
            var actualData = await GetActualData(checkInsIds: filteredTestData.Select(selector: t => t.CheckInsId)
                    .ToImmutableList())
                .ConfigureAwait(continueOnCapturedContext: false);
            Assert.That(actual: actualData.Count(predicate: a => a.CheckInDate == null), expression: Is.Zero);
            Assert.That(actual: actualData.Count, expression: Is.EqualTo(expected: filteredTestData.Count));
        }

        private static async Task<CheckInOutResult> SendRequest(
            string securityCode,
            IImmutableList<int> selectedLocationIds,
            bool isFastCheckInOut,
            CheckInOutController controller
        )
        {
            var request = new CheckInOutRequest
            {
                SecurityCode = securityCode,
                EventId = 389697,
                SelectedLocationIds = selectedLocationIds,
                IsFastCheckInOut = isFastCheckInOut,
                CheckType = CheckType.CheckIn,
                CheckInOutCandidates = ImmutableList<CheckInOutCandidate>.Empty
            };

            var actionResult = await controller.GetPeople(request: request);
            var okResult = actionResult as OkObjectResult;
            var checkInOutResult = okResult!.Value as CheckInOutResult;
            return checkInOutResult!;
        }

        private async Task<CheckInOutController> SetupTestEnvironment()
        {
            _serviceProvider = TestHelper.SetupServiceProviderWithKidsTownDi();
            await CleanDatabase().ConfigureAwait(continueOnCapturedContext: false);
            await TestHelper.InsertDefaultTestData(serviceProvider: _serviceProvider).ConfigureAwait(continueOnCapturedContext: false);

            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var updateTaskMock = new Mock<IUpdateTask>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, updateTask: updateTaskMock.Object);
            return controller;
        }

        private async Task<IImmutableList<Data>> GetActualData(IImmutableList<long> checkInsIds)
        {
            await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);
            
            var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

            await using var db = serviceScopeFactory!.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>();
            var attendances = await (from a in db.Attendances
                    where a.CheckInsId < 100 && checkInsIds.Contains(a.CheckInsId)
                    select MapData(a))
                .ToListAsync().ConfigureAwait(continueOnCapturedContext: false);

            return attendances.ToImmutableList();
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