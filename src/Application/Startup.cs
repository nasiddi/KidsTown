using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BackgroundTasks.Adult;
using BackgroundTasks.Attendance;
using BackgroundTasks.CheckOut;
using BackgroundTasks.Cleanup;
using BackgroundTasks.Common;
using BackgroundTasks.Kid;
using Database;
using Database.EfCore;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using KidsTown;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using PlanningCenterApiClient;

namespace Application;

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

        app.UseCors("AllowReactApp");

        if (!env.IsDevelopment())
        {
            var options = new RewriteOptions()
                .Add(
                    context =>
                    {
                        var request = context.HttpContext.Request;
                        var path = request.Path.Value ?? "";

                        // Don't touch paths with file extensions (static assets like .js, .css)
                        if (Path.HasExtension(path))
                        {
                            context.Result = RuleResult.ContinueRules;
                            return;
                        }

                        // Lowercase route paths
                        if (Regex.IsMatch(path, "[A-Z]"))
                        {
                            path = path.ToLowerInvariant();
                            context.HttpContext.Request.Path = path;
                        }

                        // Skip API routes (handled by controllers)
                        if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Result = RuleResult.ContinueRules;
                            return;
                        }

                        // SPA fallback: all non-file, non-API routes serve index.html
                        context.HttpContext.Request.Path = "/index.html";
                        context.Result = RuleResult.SkipRemainingRules;
                    });

            app.UseRewriter(options);

            app.UseStaticFiles();
        }

        app.UseRouting();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });
    }
}