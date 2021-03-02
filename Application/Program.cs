using CheckInsExtension.CheckInUpdateJobs.Update;
using Microsoft.AspNetCore.Hosting;
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls("http://localhost:5000");
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<UpdateTask>();
                    services.AddHostedService(p => p.GetRequiredService<UpdateTask>());
                });
    }
}