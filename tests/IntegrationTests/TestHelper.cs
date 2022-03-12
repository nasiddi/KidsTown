using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.Kid;
using KidsTown.Database;
using KidsTown.Database.EfCore;
using KidsTown.IntegrationTests.Mocks;
using KidsTown.IntegrationTests.TestData;
using KidsTown.KidsTown;
using KidsTown.PlanningCenterApiClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Person = KidsTown.Database.EfCore.Person;

namespace KidsTown.IntegrationTests;

public static class TestHelper
{
    public static async Task CleanDatabase(IServiceProvider serviceProvider)
    {
        await using var db = serviceProvider.GetRequiredService<KidsTownContext>();
        await EstablishConnectionToDatabase(db: db).ConfigureAwait(continueOnCapturedContext: false);
                
        var attendances = await db.Attendances.Where(predicate: a => 0 < a.CheckInsId && a.CheckInsId < 100).ToListAsync();
        var people = await db.People.Where(predicate: p => attendances.Select(a => a.PersonId)
                .Contains(p.Id))
            .Include(navigationPropertyPath: p => p.Kid)
            .Include(navigationPropertyPath: p => p.Adult).ToListAsync();

        var kids = people.Where(predicate: p => p.Kid != null).Select(selector: p => p.Kid);
        var adults = people.Where(predicate: p => p.Adult != null).Select(selector: p => p.Adult);
            
        db.RemoveRange(entities: kids!);
        db.RemoveRange(entities: adults!);
        db.RemoveRange(entities: attendances);
        db.RemoveRange(entities: people);
        await db.SaveChangesAsync();
    }

    private static async Task EstablishConnectionToDatabase(DbContext db)
    {
        while (!await db.Database.CanConnectAsync())
        {
            await Task.Delay(millisecondsDelay: 100).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    public static async Task InsertDefaultTestData(ServiceProvider serviceProvider)
    {
        var testData = TestDataFactory.GetTestData();
        await InsertTestData(serviceProvider: serviceProvider, testData: testData)
            .ConfigureAwait(continueOnCapturedContext: false);
    }
        
    public static async Task InsertTestData(ServiceProvider serviceProvider, IImmutableList<TestData.TestData> testData)
    {
        await using var db = serviceProvider.GetRequiredService<KidsTownContext>();
        await EstablishConnectionToDatabase(db: db).ConfigureAwait(continueOnCapturedContext: false);

        var locations = await db.Locations.ToListAsync();

        var people = testData
            .GroupBy(keySelector: d => d.PeopleId)
            .Select(selector: d => MapPerson(grouping: d, locations: locations.ToImmutableList()))
            .ToImmutableList();

        await db.AddRangeAsync(entities: people).ConfigureAwait(continueOnCapturedContext: false);
        await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
    }

    private static Attendance MapAttendance(TestData.TestData data, IImmutableList<Location> locations)
    {
        var location = locations.Single(predicate: l => l.CheckInsLocationId == (long) data.TestLocation);

        return new()
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

        var attendances = grouping.Select(selector: g => MapAttendance(data: g, locations: locations));

        var kid = new Kid
        {
            MayLeaveAlone = data.ExpectedMayLeaveAlone ?? true,
            HasPeopleWithoutPickupPermission = data.ExpectedHasPeopleWithoutPickupPermission ?? false
        };

        return new()
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
        var configuration = SetupConfigurations(services: services);
        SetupDatabaseConnection(services: services, configuration: configuration);

        if (setupKidsTownDi)
        {
            SetupKidsTownDi(services: services);
        }

        if (setupBackgroundTasksDi)
        {
            SetupBackgroundTasksDi(mockPlanningCenterClient: mockPlanningCenterClient, services: services);
        }
            
        return services.BuildServiceProvider();
    }

    private static void SetupDatabaseConnection(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KidsTownContext>(
            contextLifetime: ServiceLifetime.Transient,
            optionsAction: o
                => o.UseSqlServer(connectionString: configuration.GetConnectionString(name: "Database")));
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
            .AddJsonFile(path: "appsettings.json", optional: false)
            .AddJsonFile(path: "appsettings.Secrets.json", optional: false)
            .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(implementationFactory: _ => configuration);
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
        services.AddSingleton(serviceType: typeof(ILogger), implementationType: typeof(Logger<BackgroundTask>));
    }
}