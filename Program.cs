using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace gcp_pub_sub
{
    static class Program
    {
        private static IConfigurationRoot Configuration;

        static async Task Main(string[] args)
        {
            // for windows service default to system32 folder before .net 7
            // below line changes it to current root folder
            // Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();

            Log.Logger.Information("Starting the application GCP Pub Sub !!!");

            try
            {
                using IHost host = Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddHostedService<BUOneWorker>();
                    })
                    .Build();

                await host.RunAsync();
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception.Message, exception);
            }
        }
    }
}