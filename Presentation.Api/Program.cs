using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Race API starting...");
                
                var hostBuilder = CreateHostBuilder(args).Build();

                Log.Information("Running in environment " + environmentName);
                Log.Information("Using Db connection " + Startup.Configuration.GetConnectionString("RaceBackendDbConnection"));

                hostBuilder.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "A fatal error occured. Race API is shutting down.");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

//{
//  grant_type: 'client_credentials',
//  client_id: 'NON-INTERACTIVE-CLIENT-ID',
//  client_secret: 'NON-INTERACTIVE-CLIENT-SECRET',
//  audience: 'https://yourdomain.auth0.com/api/v2/"    }
//}

