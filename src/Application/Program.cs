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
        CreateHostBuilder(args: args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var preLoadedConfig = new ConfigurationBuilder()
            .AddJsonFile(path: "appsettings.json", optional: false)
            .AddJsonFile(path: "appsettings.Secrets.json", optional: false)
            .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
            .Build();
            
        return Host.CreateDefaultBuilder(args: args)
            .ConfigureAppConfiguration(configureDelegate: (_, config) =>
            {
                config.AddJsonFile(path: "appsettings.Secrets.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true, reloadOnChange: true);

                config.AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(configure: webBuilder =>
            {
                webBuilder
                    .UseStartup<Startup>()
                    .UseUrls(preLoadedConfig.GetValue<string>(key: "Url"));
            })
            .ConfigureServices(configureDelegate: services =>
            {
                services.AddSingleton<AttendanceUpdateTask>();
                services.AddHostedService(implementationFactory: p => p.GetRequiredService<AttendanceUpdateTask>());
                services.AddSingleton<KidUpdateTask>();
                services.AddHostedService(implementationFactory: p => p.GetRequiredService<KidUpdateTask>());
                services.AddSingleton<AutoCheckOutTask>();
                services.AddHostedService(implementationFactory: p => p.GetRequiredService<AutoCheckOutTask>());
                services.AddSingleton<AdultUpdateTask>();
                services.AddHostedService(implementationFactory: p => p.GetRequiredService<AdultUpdateTask>());
                services.AddSingleton<OldLogCleanupTask>();
                services.AddHostedService(implementationFactory: p => p.GetRequiredService<OldLogCleanupTask>());
            });
    }
            
}