using KidsTown.BackgroundTasks.Adult;
using KidsTown.BackgroundTasks.Attendance;
using KidsTown.BackgroundTasks.CheckOut;
using KidsTown.BackgroundTasks.Cleanup;
using KidsTown.BackgroundTasks.Kid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KidsTown.Application;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var preLoadedConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Secrets.json", optional: false)
            .AddJsonFile("appsettings.DevelopementMachine.json", optional: true)
            .Build();

        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(
                (_, config) =>
                {
                    config.AddJsonFile("appsettings.Secrets.json", optional: false, reloadOnChange: true)
                        .AddJsonFile("appsettings.DevelopementMachine.json", optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();
                })
            .ConfigureWebHostDefaults(
                webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls(preLoadedConfig.GetValue<string>("Url")!);
                })
            .ConfigureServices(
                services =>
                {
                    services.AddSingleton<AttendanceUpdateTask>();
                    services.AddHostedService(p => p.GetRequiredService<AttendanceUpdateTask>());
                    services.AddSingleton<KidUpdateTask>();
                    services.AddHostedService(p => p.GetRequiredService<KidUpdateTask>());
                    services.AddSingleton<AutoCheckOutTask>();
                    services.AddHostedService(p => p.GetRequiredService<AutoCheckOutTask>());
                    services.AddSingleton<AdultUpdateTask>();
                    services.AddHostedService(p => p.GetRequiredService<AdultUpdateTask>());
                    services.AddSingleton<OldLogCleanupTask>();
                    services.AddHostedService(p => p.GetRequiredService<OldLogCleanupTask>());
                });
    }
}