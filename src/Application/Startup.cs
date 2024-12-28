using System;
using System.IO;
using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
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
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace KidsTown.Application;

public class Startup(IConfiguration configuration)
{
    private IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews().AddJsonOptions(opts => { opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

        services.AddSingleton<Func<BackgroundTaskType, IBackgroundTask>>(
            serviceProvider =>
                key =>
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
        services.AddSingleton<IUserRepository, UserRepository>();

        services.AddDbContext<KidsTownContext>(
            o
                => o.UseSqlServer(Configuration.GetConnectionString("Database")!));

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
        services.AddScoped<IDocumentationRepository, DocumentationRepository>();
        services.AddScoped<IDocumentationService, DocumentationService>();

        var credential = GoogleCredential.FromFile(Configuration.GetValue<string>("GoogleSecretsFile"))
            .CreateScoped(DriveService.ScopeConstants.Drive);

        var service = new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

        services.AddSingleton(service);

        services.AddCors(
            options =>
            {
                options.AddPolicy(
                    "AllowReactApp",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetPreflightMaxAge(TimeSpan.FromMinutes(minutes: 120));
                    });
            });
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

        var options = new RewriteOptions()
            // Rewrite only exact matches to .html
            .AddRewrite(@"^$", "index.html", skipRemainingRules: true)
            .AddRewrite(@"^checkin/?$", "checkin.html", skipRemainingRules: true)
            .AddRewrite(@"^documentation/?$", "documentation.html", skipRemainingRules: true)
            .AddRewrite(@"^login/?$", "login.html", skipRemainingRules: true)
            .AddRewrite(@"^overview/?$", "overview.html", skipRemainingRules: true)
            .AddRewrite(@"^settings/?$", "settings.html", skipRemainingRules: true)
            .AddRewrite(@"^settings/documentation/?$", "settings/documentation.html", skipRemainingRules: true)
            .AddRewrite(@"^statistic/?$", "statistic.html", skipRemainingRules: true);

        app.UseRewriter(options);

        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors("AllowReactApp");

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });

        app.UseStaticFiles(
            new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "wwwroot")),
                RequestPath = ""
            });
    }
}