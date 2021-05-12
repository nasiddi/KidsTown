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

namespace KidsTown.IntegrationTests
{
    public static class TestHelper
    {
        public static async Task CleanDatabase(IServiceProvider serviceProvider)
        {
            await using var db = serviceProvider!.GetRequiredService<KidsTownContext>();
            await EstablishConnectionToDatabase(db).ConfigureAwait(false);
                
            var attendances = await db.Attendances.Where(a => a.CheckInsId < 100).ToListAsync();
            var people = await db.People.Where(p => attendances.Select(a => a.PersonId)
                .Contains(p.Id))
                .Include(p => p.Kid)
                .Include(p => p.Adult).ToListAsync();

            var kids = people.Where(p => p.Kid != null).Select(p => p.Kid);
            var adults = people.Where(p => p.Adult != null).Select(p => p.Adult);
            
            db.RemoveRange(kids);
            db.RemoveRange(adults);
            db.RemoveRange(attendances);
            db.RemoveRange(people);
            await db.SaveChangesAsync();
        }

        private static async Task EstablishConnectionToDatabase(DbContext db)
        {
            while (!await db.Database.CanConnectAsync())
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        public static async Task InsertDefaultTestData(ServiceProvider serviceProvider)
        {
            var testData = TestDataFactory.GetTestData();
            await InsertTestData(serviceProvider: serviceProvider, testData: testData)
                .ConfigureAwait(false);
        }
        
        public static async Task InsertTestData(ServiceProvider serviceProvider, IImmutableList<TestData.TestData> testData)
        {
            await using var db = serviceProvider!.GetRequiredService<KidsTownContext>();
            await EstablishConnectionToDatabase(db).ConfigureAwait(false);

            var locations = await db.Locations.ToListAsync();

            var people = testData
                .GroupBy(d => d.PeopleId)
                .Select(d => MapPerson(grouping: d, locations: locations.ToImmutableList()))
                .ToImmutableList();

            await db.AddRangeAsync(people).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
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

            var attendances = grouping.Select(g => MapAttendance(data: g, locations: locations));

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
            SetupDatabaseConnection(services: services, configuration: configuration);

            if (setupKidsTownDi)
            {
                SetupKidsTownDi(services);
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
        }
        
        private static IConfigurationRoot SetupConfigurations(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(path: "appsettings.json", optional: false)
                .AddJsonFile(path: "appsettings.Secrets.json", optional: false)
                .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
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
            services.AddSingleton(serviceType: typeof(ILogger), implementationType: typeof(Logger<BackgroundTask>));
        }
    }
}