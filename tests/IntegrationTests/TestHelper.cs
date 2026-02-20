using System.Collections.Immutable;
using BackgroundTasks.Adult;
using BackgroundTasks.Attendance;
using BackgroundTasks.Common;
using BackgroundTasks.Kid;
using Database;
using Database.EfCore;
using IntegrationTests.Mocks;
using IntegrationTests.TestData;
using KidsTown;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlanningCenterApiClient;
using Person = Database.EfCore.Person;

namespace IntegrationTests;

public static class TestHelper
{
    public static async Task CleanDatabase(IServiceProvider serviceProvider)
    {
        await using var db = serviceProvider.GetRequiredService<KidsTownContext>();
        await EstablishConnectionToDatabase(db);

        var attendances = await db.Attendances.Where(a => 0 < a.CheckInsId && a.CheckInsId < 100).ToListAsync();
        var people = await db.People.Where(
                p => attendances.Select(a => a.PersonId)
                    .Contains(p.Id))
            .Include(p => p.Kid)
            .Include(p => p.Adult)
            .ToListAsync();

        var kids = people.Where(p => p.Kid != null).Select(p => p.Kid);
        var adults = people.Where(p => p.Adult != null).Select(p => p.Adult);

        db.RemoveRange(kids!);
        db.RemoveRange(adults!);
        db.RemoveRange(attendances);
        db.RemoveRange(people);
        await db.SaveChangesAsync();
    }

    private static async Task EstablishConnectionToDatabase(DbContext db)
    {
        while (!await db.Database.CanConnectAsync())
        {
            await Task.Delay(millisecondsDelay: 100);
        }
    }

    public static async Task InsertDefaultTestData(ServiceProvider serviceProvider)
    {
        var testData = TestDataFactory.GetTestData();
        await InsertTestData(serviceProvider, testData);
    }

    public static async Task InsertTestData(ServiceProvider serviceProvider, IImmutableList<TestData.TestData> testData)
    {
        await using var db = serviceProvider.GetRequiredService<KidsTownContext>();
        await EstablishConnectionToDatabase(db);

        var locations = await db.Locations.ToListAsync();

        var people = testData
            .GroupBy(d => d.PeopleId)
            .Select(d => MapPerson(d, locations.ToImmutableList()))
            .ToImmutableList();

        await db.AddRangeAsync(people);
        await db.SaveChangesAsync();
    }

    private static Attendance MapAttendance(TestData.TestData data, IImmutableList<Location> locations)
    {
        var location = locations.Single(l => l.CheckInsLocationId == (long) data.TestLocation);

        return new Attendance
        {
            CheckInsId = data.CheckInsId,
            LocationId = location.Id,
            SecurityCode = data.SecurityCode,
            AttendanceTypeId = (int) data.AttendanceType + 1,
            InsertDate = DateTime.UtcNow,
            CheckInDate = null,
            CheckOutDate = null
        };
    }

    private static Person MapPerson(IGrouping<long?, TestData.TestData> grouping, IImmutableList<Location> locations)
    {
        var data = grouping.First();

        var attendances = grouping.Select(g => MapAttendance(g, locations));

        var kid = new Kid
        {
            MayLeaveAlone = data.ExpectedMayLeaveAlone ?? true,
            HasPeopleWithoutPickupPermission = data.ExpectedHasPeopleWithoutPickupPermission ?? false
        };

        return new Person
        {
            PeopleId = data.PeopleId,
            FirstName = data.PeopleFirstName ?? data.CheckInsFirstName,
            LastName = data.PeopleLastName ?? data.CheckInsLastName,
            Attendances = attendances.ToList(),
            Kid = kid
        };
    }

    public static ServiceProvider SetupServiceProviderWithKidsTownDi()
    {
        return SetupServiceProvider(
            setupKidsTownDi: true,
            setupBackgroundTasksDi: false,
            mockPlanningCenterClient: false);
    }

    public static ServiceProvider SetupServiceProviderWithBackgroundTasksDi()
    {
        return SetupServiceProvider(
            setupKidsTownDi: false,
            setupBackgroundTasksDi: true,
            mockPlanningCenterClient: false);
    }

    public static ServiceProvider SetupServiceProviderWithBackgroundTasksDiAndMockedPlanningCenterClient()
    {
        return SetupServiceProvider(
            setupKidsTownDi: false,
            setupBackgroundTasksDi: true,
            mockPlanningCenterClient: true);
    }

    private static ServiceProvider SetupServiceProvider(
        bool setupKidsTownDi,
        bool setupBackgroundTasksDi,
        bool mockPlanningCenterClient
    )
    {
        IServiceCollection services = new ServiceCollection();
        var configuration = SetupConfigurations(services);
        SetupDatabaseConnection(services, configuration);

        if (setupKidsTownDi)
        {
            SetupKidsTownDi(services);
        }

        if (setupBackgroundTasksDi)
        {
            SetupBackgroundTasksDi(mockPlanningCenterClient, services);
        }

        return services.BuildServiceProvider();
    }

    private static void SetupDatabaseConnection(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KidsTownContext>(
            contextLifetime: ServiceLifetime.Transient,
            optionsAction: o
                => o.UseSqlServer(configuration.GetConnectionString("Database")));
    }

    private static void SetupKidsTownDi(IServiceCollection services)
    {
        services.AddScoped<ICheckInOutService, CheckInOutService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IOverviewService, OverviewService>();

        services.AddScoped<ICheckInOutRepository, CheckInOutRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IOverviewRepository, OverviewRepository>();

        services.AddScoped<IPeopleRepository, PeopleRepository>();
    }

    private static IConfigurationRoot SetupConfigurations(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Secrets.json", optional: false)
            .AddJsonFile("appsettings.DevelopementMachine.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(_ => configuration);
        return configuration;
    }

    private static void SetupBackgroundTasksDi(bool mockPlanningCenterClient, IServiceCollection services)
    {
        services.AddSingleton<AttendanceUpdateTask>();

        services.AddSingleton<KidUpdateTask>();

        services.AddSingleton<AdultUpdateTask>();

        if (mockPlanningCenterClient)
        {
            services.AddSingleton<IPlanningCenterClient, PlanningCenterClientMock>();
        }
        else
        {
            services.AddSingleton<IPlanningCenterClient, PlanningCenterClient>();
        }

        services.AddSingleton<IAttendanceUpdateService, AttendanceUpdateService>();
        services.AddSingleton<IKidUpdateService, KidUpdateService>();
        services.AddSingleton<IAdultUpdateService, AdultUpdateService>();

        services.AddSingleton<IAttendanceUpdateRepository, AttendanceUpdateRepository>();
        services.AddSingleton<IKidUpdateRepository, KidUpdateRepository>();
        services.AddSingleton<IAdultUpdateRepository, AdultUpdateRepository>();
        services.AddSingleton<IBackgroundTaskRepository, BackgroundTaskRepository>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger), typeof(Logger<BackgroundTask>));
    }
}