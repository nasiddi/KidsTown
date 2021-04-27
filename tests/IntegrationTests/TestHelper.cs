using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.BackgroundTasks.PlanningCenter;
using KidsTown.Database;
using KidsTown.IntegrationTests.Mocks;
using KidsTown.IntegrationTests.TestData;
using KidsTown.KidsTown;
using KidsTown.PlanningCenterApiClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KidsTown.IntegrationTests
{
    public static class TestHelper
    {
        public static async Task CleanDatabase(IServiceProvider serviceProvider)
        {
            await using var db = serviceProvider!.GetRequiredService<KidsTownContext>();
            await EstablishConnectionToDatabase(db: db).ConfigureAwait(continueOnCapturedContext: false);
                
            var attendances = await db.Attendances.Where(predicate: a => a.CheckInsId < 100).ToListAsync();
            var people = await db.Kids.Where(predicate: p => attendances.Select(a => a.KidId)
                .Contains(p.Id)).ToListAsync();
                
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
            await using var db = serviceProvider!.GetRequiredService<KidsTownContext>();
            await EstablishConnectionToDatabase(db: db).ConfigureAwait(continueOnCapturedContext: false);

            var locations = await db.Locations.ToListAsync();

            var people = testData
                .GroupBy(keySelector: d => d.PeopleId)
                .Select(selector: d => MapKid(grouping: d, locations: locations.ToImmutableList()))
                .ToImmutableList();

            await db.AddRangeAsync(entities: people).ConfigureAwait(continueOnCapturedContext: false);
            await db.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        private static Attendance MapAttendance(TestData.TestData data, ImmutableList<Location> locations)
        {
            var location = locations.Single(predicate: l => l.CheckInsLocationId == (long) data.TestLocation);

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

        private static Kid MapKid(IGrouping<long?, TestData.TestData> grouping, ImmutableList<Location> locations)
        {
            var data = grouping.First();

            var attendances = grouping.Select(selector: g => MapAttendance(data: g, locations: locations));
            
            return new Kid
            {
                PeopleId = data.PeopleId,
                FistName = data.PeopleFirstName ?? data.CheckInsFirstName,
                LastName = data.PeopleLastName ?? data.CheckInsLastName,
                MayLeaveAlone = data.ExpectedMayLeaveAlone ?? true,
                HasPeopleWithoutPickupPermission = data.ExpectedHasPeopleWithoutPickupPermission ?? false,
                Attendances = attendances.ToList()
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
        }
    }
}