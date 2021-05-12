using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Application.Controllers;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.Common;
using KidsTown.Database;
using KidsTown.Database.EfCore;
using KidsTown.IntegrationTests.TestData;
using KidsTown.KidsTown;
using KidsTown.Shared;
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
            await Task.Delay(1000).ConfigureAwait(false);
            await CleanDatabase().ConfigureAwait(false);
        }

        [Test]
        public async Task GetPeople_ExistingData_AllFound()
        {
            // Arrange
            var controller = await SetupTestEnvironment().ConfigureAwait(false);
            var testData = TestDataFactory.GetTestData();

            // Act & Assert
            (testData as ImmutableList<TestData.TestData>)?.ForEach(async t =>
            {
                var checkInOutResult = await SendRequest(
                    securityCode: t.SecurityCode,
                    selectedLocationIds: ImmutableList.Create(t.LocationGroupId),
                    isFastCheckInOut: false,
                    controller: controller)
                    .ConfigureAwait(false);
                
                var candidates = checkInOutResult!.CheckInOutCandidates.Select(c => c.Name).ToArray<object>();
                Assert.That(actual: $"{t.PeopleFirstName ?? t.CheckInsFirstName} {t.PeopleLastName ?? t.CheckInsLastName}", 
                    expression: Is.AnyOf(candidates));
            });
        }

        [Test]
        public async Task GetPeople_NoLocationGroupSet_NothingFound()
        {
            // Arrange
            var controller = await SetupTestEnvironment().ConfigureAwait(false);
            var testData = TestDataFactory.GetTestData();
            
            // Act & Assert
            (testData as ImmutableList<TestData.TestData>)?.ForEach(async t =>
            {
                var checkInOutResult = await SendRequest(
                    securityCode: t.SecurityCode,
                    selectedLocationIds: ImmutableList<int>.Empty,
                    isFastCheckInOut: false,
                    controller: controller)
                    .ConfigureAwait(false);

                Assert.That(actual: checkInOutResult.AlertLevel, expression: Is.EqualTo(AlertLevel.Danger));
            });
        }

        [Test]
        public async Task GetPeople_FastCheckInActive_CheckedIn()
        {
            // Arrange
            _serviceProvider = TestHelper.SetupServiceProviderWithKidsTownDi();
            await CleanDatabase().ConfigureAwait(false);
            
            var testData = TestDataFactory.GetTestData().GroupBy(t => t.SecurityCode).ToImmutableList();
            var filteredTestData = testData.Where(t => t.Count() == 1)
                .SelectMany(g => g.ToImmutableList())
                .Where(a => a.PeopleId == 1)
                .ToImmutableList();
            
            await TestHelper.InsertTestData(serviceProvider: _serviceProvider, testData: filteredTestData);
            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var taskManagementServiceMock = new Mock<ITaskManagementService>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, taskManagementService: taskManagementServiceMock.Object);
            
            // Act
            filteredTestData.ForEach(async t =>
            {
                var checkInOutResult = await SendRequest(
                    securityCode: t.SecurityCode,
                    selectedLocationIds: ImmutableList.Create(t.LocationGroupId),
                    isFastCheckInOut: true,
                    controller: controller)
                    .ConfigureAwait(false);

                Assert.That(actual: checkInOutResult.AlertLevel, expression: Is.EqualTo(AlertLevel.Success));
                Assert.That(actual: checkInOutResult.SuccessfulFastCheckout, expression: Is.True);
            });

            // Assert
            var actualData = await GetActualData(filteredTestData.Select(t => t.CheckInsId)
                    .ToImmutableList())
                .ConfigureAwait(false);
            Assert.That(actual: actualData.Count(a => a.CheckInDate == null), expression: Is.Zero);
            Assert.That(actual: actualData.Count, expression: Is.EqualTo(filteredTestData.Count));
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

            var actionResult = await controller.GetPeople(request);
            var okResult = actionResult as OkObjectResult;
            var checkInOutResult = okResult!.Value as CheckInOutResult;
            return checkInOutResult!;
        }

        private async Task<CheckInOutController> SetupTestEnvironment()
        {
            _serviceProvider = TestHelper.SetupServiceProviderWithKidsTownDi();
            await CleanDatabase().ConfigureAwait(false);
            await TestHelper.InsertDefaultTestData(_serviceProvider).ConfigureAwait(false);

            var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
            var taskManagementServiceMock = new Mock<ITaskManagementService>();
            var controller = new CheckInOutController(checkInOutService: checkInOutService!, taskManagementService: taskManagementServiceMock.Object);
            return controller;
        }

        private async Task<IImmutableList<Data>> GetActualData(IImmutableList<long> checkInsIds)
        {
            await Task.Delay(500).ConfigureAwait(false);
            
            var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

            await using var db = serviceScopeFactory!.CreateScope().ServiceProvider
                .GetRequiredService<KidsTownContext>();
            var attendances = await (from a in db.Attendances
                    where a.CheckInsId < 100 && checkInsIds.Contains(a.CheckInsId)
                    select MapData(a))
                .ToListAsync().ConfigureAwait(false);

            return attendances.ToImmutableList();
        }

        private static Data MapData(Attendance attendance) => new(attendance.CheckInDate);

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
            await TestHelper.CleanDatabase(_serviceProvider);
        }
    }
}