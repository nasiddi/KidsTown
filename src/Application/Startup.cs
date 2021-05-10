using System;
using System.Text.Json.Serialization;
using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.CheckOut;
using KidsTown.BackgroundTasks.Common;
using KidsTown.BackgroundTasks.Kid;
using KidsTown.Database;
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
                AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

            services.AddSingleton<Func<BackgroundTaskType, IBackgroundTask>>(serviceProvider => key =>
            {
                return key switch
                {
                    BackgroundTaskType.AdultUpdateTask => serviceProvider.GetService<AdultUpdateTask>()!,
                    BackgroundTaskType.AttendanceUpdateTask => serviceProvider.GetService<AttendanceUpdateTask>()!,
                    BackgroundTaskType.AutoCheckOutTask => serviceProvider.GetService<AutoCheckOutTask>()!,
                    BackgroundTaskType.KidUpdateTask => serviceProvider.GetService<KidUpdateTask>()!,
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

            services.AddDbContext<KidsTownContext>(o 
                => o.UseSqlServer(Configuration.GetConnectionString("Database")));

            services.AddScoped<ICheckInOutService, CheckInOutService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IOverviewService, OverviewService>();
            services.AddScoped<IPeopleService, PeopleService>();
            
            services.AddScoped<ICheckInOutRepository, CheckInOutRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IOverviewRepository, OverviewRepository>();
            services.AddScoped<IPeopleRepository, PeopleRepository>();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer("start");
                }
            });
        }
    }
}