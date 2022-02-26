using System;
using System.Text.Json.Serialization;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.CheckOut;
using KidsTown.BackgroundTasks.Cleanup;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.Kid;
using KidsTown.Database;
using KidsTown.Database.EfCore;
using KidsTown.KidsTown;
using KidsTown.PlanningCenterApiClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KidsTown.Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().
                AddJsonOptions(configure: opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(item: new JsonStringEnumConverter());
                });
            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration: configuration => { configuration.RootPath = "ClientApp/build"; });

            services.AddSingleton<Func<BackgroundTaskType, IBackgroundTask>>(implementationFactory: serviceProvider => key =>
            {
                return key switch
                {
                    BackgroundTaskType.AdultUpdateTask => serviceProvider.GetService<AdultUpdateTask>()!,
                    BackgroundTaskType.AttendanceUpdateTask => serviceProvider.GetService<AttendanceUpdateTask>()!,
                    BackgroundTaskType.AutoCheckOutTask => serviceProvider.GetService<AutoCheckOutTask>()!,
                    BackgroundTaskType.KidUpdateTask => serviceProvider.GetService<KidUpdateTask>()!,
                    BackgroundTaskType.OldLogCleanupTask => serviceProvider.GetService<OldLogCleanupTask>()!,
                    _ => null!
                };
            });

            services.AddSingleton<ITaskManagementService, TaskManagementService>();
            
            services.AddSingleton<IPlanningCenterClient, PlanningCenterClient>();
            
            services.AddSingleton<IAttendanceUpdateService, AttendanceUpdateService>();
            services.AddSingleton<IAdultUpdateService, AdultUpdateService>();
            services.AddSingleton<IKidUpdateService, KidUpdateService>();
            
            services.AddSingleton<IAttendanceUpdateRepository, AttendanceUpdateRepository>();
            services.AddSingleton<IKidUpdateRepository, KidUpdateRepository>();
            services.AddSingleton<IAdultUpdateRepository, AdultUpdateRepository>();
            services.AddSingleton<IBackgroundTaskRepository, BackgroundTaskRepository>();
            services.AddSingleton<ISearchLogCleanupRepository, SearchLoggingRepository>();

            services.AddDbContext<KidsTownContext>(optionsAction: o 
                => o.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "Database")));

            services.AddScoped<ICheckInOutService, CheckInOutService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IOverviewService, OverviewService>();
            services.AddScoped<IPeopleService, PeopleService>();
            services.AddScoped<ISearchLoggingService, SearchLoggingService>();
            
            services.AddScoped<ICheckInOutRepository, CheckInOutRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IOverviewRepository, OverviewRepository>();
            services.AddScoped<IPeopleRepository, PeopleRepository>();
            services.AddScoped<ISearchLoggingRepository, SearchLoggingRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorHandlingPath: "/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(configure: endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(configuration: spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}