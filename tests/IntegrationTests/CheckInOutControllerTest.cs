using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.Application.Controllers;
using KidsTown.Application.Models;
using KidsTown.BackgroundTasks.Common;
using KidsTown.Database.EfCore;
using KidsTown.IntegrationTests.TestData;
using KidsTown.KidsTown;
using KidsTown.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace KidsTown.IntegrationTests;

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
        (testData as ImmutableList<TestData.TestData>)?.ForEach(
            async t =>
            {
                var checkInOutResult = await SendRequest(
                        t.SecurityCode,
                        ImmutableList.Create(t.LocationGroupId),
                        isFastCheckInOut: false,
                        controller)
                    .ConfigureAwait(continueOnCapturedContext: false);

                var candidates = checkInOutResult.CheckInOutCandidates.Select(c => c.Name).ToArray<object>();
                Assert.That(
                    $"{t.PeopleFirstName ?? t.CheckInsFirstName} {t.PeopleLastName ?? t.CheckInsLastName}",
                    Is.AnyOf(candidates));
            });
    }

    [Test]
    public async Task GetPeople_NoLocationGroupSet_NothingFound()
    {
        // Arrange
        var controller = await SetupTestEnvironment().ConfigureAwait(continueOnCapturedContext: false);
        var testData = TestDataFactory.GetTestData();

        // Act & Assert
        (testData as ImmutableList<TestData.TestData>)?.ForEach(
            async t =>
            {
                var checkInOutResult = await SendRequest(
                        t.SecurityCode,
                        ImmutableList<int>.Empty,
                        isFastCheckInOut: false,
                        controller)
                    .ConfigureAwait(continueOnCapturedContext: false);

                Assert.That(checkInOutResult.AlertLevel, Is.EqualTo(AlertLevel.Error));
            });
    }

    [Test]
    public async Task GetPeople_FastCheckInActive_CheckedIn()
    {
        // Arrange
        _serviceProvider = TestHelper.SetupServiceProviderWithKidsTownDi();
        await CleanDatabase().ConfigureAwait(continueOnCapturedContext: false);

        var testData = TestDataFactory.GetTestData().GroupBy(t => t.SecurityCode).ToImmutableList();
        var filteredTestData = testData.Where(t => t.Count() == 1)
            .SelectMany(g => g.ToImmutableList())
            .Where(a => a.PeopleId == 1)
            .ToImmutableList();

        await TestHelper.InsertTestData(_serviceProvider, filteredTestData);
        var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
        var taskManagementServiceMock = new Mock<ITaskManagementService>();
        var searchLoggingServiceMock = new Mock<ISearchLoggingService>();
        var controller = new CheckInOutController(
            checkInOutService!,
            taskManagementServiceMock.Object,
            searchLoggingServiceMock.Object);

        // Act
        filteredTestData.ForEach(
            async t =>
            {
                var checkInOutResult = await SendRequest(
                        t.SecurityCode,
                        ImmutableList.Create(t.LocationGroupId),
                        isFastCheckInOut: true,
                        controller)
                    .ConfigureAwait(continueOnCapturedContext: false);

                Assert.That(checkInOutResult.AlertLevel, Is.EqualTo(AlertLevel.Success));
                Assert.That(checkInOutResult.SuccessfulFastCheckout, Is.True);
            });

        // Assert
        var actualData = await GetActualData(
                filteredTestData.Select(t => t.CheckInsId)
                    .ToImmutableList())
            .ConfigureAwait(continueOnCapturedContext: false);

        Assert.That(actualData.Count(a => a.CheckInDate == null), Is.Zero);
        Assert.That(actualData.Count, Is.EqualTo(filteredTestData.Count));
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
            SelectedLocationGroupIds = selectedLocationIds,
            IsFastCheckInOut = isFastCheckInOut,
            CheckType = CheckType.CheckIn,
            CheckInOutCandidates = ImmutableList<CheckInOutCandidate>.Empty,
            FilterLocations = true
        };

        var actionResult = await controller.GetPeople(request);
        var okResult = actionResult as OkObjectResult;
        var checkInOutResult = okResult!.Value as CheckInOutResult;
        return checkInOutResult!;
    }

    private async Task<CheckInOutController> SetupTestEnvironment()
    {
        _serviceProvider = TestHelper.SetupServiceProviderWithKidsTownDi();
        await CleanDatabase().ConfigureAwait(continueOnCapturedContext: false);
        await TestHelper.InsertDefaultTestData(_serviceProvider).ConfigureAwait(continueOnCapturedContext: false);

        var checkInOutService = _serviceProvider.GetService<ICheckInOutService>();
        var taskManagementServiceMock = new Mock<ITaskManagementService>();
        var searchLoggingServiceMock = new Mock<ISearchLoggingService>();
        var controller = new CheckInOutController(
            checkInOutService!,
            taskManagementServiceMock.Object,
            searchLoggingServiceMock.Object);

        return controller;
    }

    private async Task<IImmutableList<Data>> GetActualData(IImmutableList<long> checkInsIds)
    {
        await Task.Delay(millisecondsDelay: 500).ConfigureAwait(continueOnCapturedContext: false);

        var serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

        await using var db = serviceScopeFactory!.CreateScope()
            .ServiceProvider
            .GetRequiredService<KidsTownContext>();

        var attendances = await (from a in db.Attendances
                where a.CheckInsId < 100 && checkInsIds.Contains(a.CheckInsId)
                select MapData(a))
            .ToListAsync()
            .ConfigureAwait(continueOnCapturedContext: false);

        return attendances.ToImmutableList();
    }

    private static Data MapData(Attendance attendance)
    {
        return new Data(attendance.CheckInDate);
    }

    private async Task CleanDatabase()
    {
        await TestHelper.CleanDatabase(_serviceProvider);
    }

    private class Data(DateTime? checkInDate)
    {
        public readonly DateTime? CheckInDate = checkInDate;
    }
}