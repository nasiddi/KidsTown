using KidsTown.BackgroundTasks.PlanningCenter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KidsTown.Application
{
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
                .AddJsonFile(path: "appsettings.Secrets.json", optional:false)
                .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true)
                .Build();
            
            return Host.CreateDefaultBuilder(args: args)
                .ConfigureAppConfiguration(configureDelegate: (_, config) =>
                {
                    config.AddJsonFile(path: "appsettings.Secrets.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: "appsettings.DevelopementMachine.json", optional: true, reloadOnChange:true);

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
                    services.AddSingleton<UpdateTask>();
                    services.AddHostedService(implementationFactory: p => p.GetRequiredService<UpdateTask>());
                });
        }
            
    }
}