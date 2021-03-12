using CheckInsExtension.CheckInUpdateJobs.Update;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var preLoadedConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Secrets.json", false)
                .AddJsonFile("appsettings.DevelopementMachine.json", true)
                .Build();
            
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.Secrets.json", optional: false, reloadOnChange: true)
                        .AddJsonFile("appsettings.DevelopementMachine.json", true, true);

                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls(preLoadedConfig.GetValue<string>("Url"));
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<UpdateTask>();
                    services.AddHostedService(p => p.GetRequiredService<UpdateTask>());
                });
        }
            
    }
}