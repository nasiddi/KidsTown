using System.Collections.Immutable;
using System.Diagnostics;
using BackgroundTasks.Adult;
using BackgroundTasks.Attendance;
using BackgroundTasks.Common;
using BackgroundTasks.Kid;
using Database.EfCore;
using FluentAssertions;
using IntegrationTests.Mocks;
using KidsTown.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PlanningCenterApiClient.Models.CheckInsResult;

namespace IntegrationTests;

public class UpdateTaskTest
{
    private IServiceProvider serviceProvider = null!;

    [TearDown]
    public async Task TearDown()
    {
        await TestHelper.CleanDatabase(serviceProvider);
    }

    [Test]
    public async Task TaskRunsWithoutExceptions()
    {
        serviceProvider = TestHelper.SetupServiceProviderWithBackgroundTasksDi();

        var updateTask = serviceProvider.GetService<AttendanceUpdateTask>();
        await RunTask(updateTask!, minExecutionCount: 2);

        Assert.That(updateTask!.GetExecutionCount(), Is.GreaterThan(expected: 1));
    }

    [Test]
    public async Task UpdateMockData()
    {
        serviceProvider = TestHelper.SetupServiceProviderWithBackgroundTasksDiAndMockedPlanningCenterClient();
        await TestHelper.CleanDatabase(serviceProvider);

        var attendanceUpdateTask = serviceProvider.GetService<AttendanceUpdateTask>();
        await RunTask(attendanceUpdateTask!);

        var kidUpdateTask = serviceProvider.GetService<KidUpdateTask>();
        await RunTask(kidUpdateTask!);

        var adultUpdateTask = serviceProvider.GetService<AdultUpdateTask>();
        await RunTask(adultUpdateTask!);

        await AssertUpdateTask();
    }

    private static async Task RunTask(BackgroundTask backgroundTask, int minExecutionCount = 1)
    {
        backgroundTask.ActivateTask();

        var task = backgroundTask.StartAsync(CancellationToken.None);

        var watch = Stopwatch.StartNew();
        while (minExecutionCount > backgroundTask.GetExecutionCount() && watch.ElapsedMilliseconds < 60000)
        {
            await Task.Delay(millisecondsDelay: 100);
        }

        backgroundTask.DeactivateTask();
        await task;
    }

    private async Task<IImmutableList<Data>> GetActualData()
    {
        var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();

        await using var db = serviceScopeFactory!.CreateScope().ServiceProvider.GetRequiredService<KidsTownContext>();

        var people = await db.Attendances
            .Include(i => i.Person)
            .Include(i => i.Person.Kid)
            .Include(i => i.Location)
            .Where(p => p.CheckInsId < 100)
            .Select(p => MapData(p))
            .ToListAsync();

        return people.ToImmutableList();
    }

    private async Task AssertUpdateTask()
    {
        var expectedData = GetExpectedData();
        var actualData = await GetActualData();

        (expectedData as ImmutableList<Data>)?.ForEach(
            e => AssertAttendance(
                e,
                actualData.SingleOrDefault(a => a.CheckInsId == e.CheckInsId)));
    }

    private static void AssertAttendance(Data expected, Data? actual)
    {
        Assert.That(actual, Is.Not.Null);
        actual.Should().BeEquivalentTo(expected);
    }

    private static Data MapData(Attendance attendance)
    {
        return new Data(
            attendance.Person.FirstName,
            attendance.Person.LastName,
            attendance.CheckInsId,
            attendance.Person.PeopleId,
            (AttendanceTypeId) attendance.AttendanceTypeId,
            attendance.Location.CheckInsLocationId!.Value,
            attendance.Person.Kid?.MayLeaveAlone ?? true,
            attendance.Person.Kid?.HasPeopleWithoutPickupPermission ?? false);
    }

    private static IImmutableList<Data> GetExpectedData()
    {
        var attendeesData = PlanningCenterClientMock.GetAttendanceData();
        var peopleData = PlanningCenterClientMock.GetKidsData();

        return attendeesData.Select(
                a =>
                {
                    var kid = peopleData.SingleOrDefault(p => p.PeopleId == a.PeopleId);

                    return new Data(
                        kid?.FirstName ?? a.FirstName,
                        kid?.LastName ?? a.LastName,
                        a.CheckInsId,
                        a.PeopleId,
                        MapAttendanceType(a.AttendanceType),
                        (int) a.TestLocation,
                        kid?.MayLeaveAlone ?? true,
                        kid?.HasPeopleWithoutPickupPermission ?? false);
                })
            .ToImmutableList();
    }

    private static AttendanceTypeId MapAttendanceType(AttendeeType attendeeType)
    {
        return attendeeType switch
        {
            AttendeeType.Guest => AttendanceTypeId.Guest,
            AttendeeType.Regular => AttendanceTypeId.Regular,
            AttendeeType.Volunteer => AttendanceTypeId.Volunteer,
            _ => throw new ArgumentOutOfRangeException(nameof(attendeeType), attendeeType, message: null)
        };
    }

    private record Data(
        string FirstName,
        string LastName,
        long CheckInsId,
        long? PeopleId,
        AttendanceTypeId AttendanceTypeId,
        long TestLocation,
        bool MayLeaveAlone,
        bool HasPeopleWithoutPickupPermission);
}