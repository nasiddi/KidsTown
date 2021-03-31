using System.Text.Json.Serialization;
using CheckInsExtension.CheckInUpdateJobs.People;
using CheckInsExtension.CheckInUpdateJobs.Update;
using CheckInsExtension.PlanningCenterAPIClient;
using ChekInsExtension.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application
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

            services.AddSingleton<IUpdateTask>(x => x.GetRequiredService<UpdateTask>());
            services.AddSingleton<IPlanningCenterClient, PlanningCenterClient>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<IUpdateRepository, UpdateRepository>();

            services.AddDbContext<CheckInsExtensionContext>(optionsAction: o 
                => o.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "Database")));

            services.AddScoped<ICheckInOutService, CheckInOutService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IOverviewService, OverviewService>();
            
            services.AddScoped<ICheckInOutRepository, CheckInOutRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IOverviewRepository, OverviewRepository>();
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