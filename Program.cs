using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services;
using System.IO;

class Program {
  static async Task Main(string[] args) => await Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) => {
          config.Sources.Clear();
          config.AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".config", 
                "statusbar", 
                "appsettings.json"), optional: false, reloadOnChange: true);
          config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
          config.AddEnvironmentVariables();
        })
        .ConfigureLogging(logging => {
          logging.AddSimpleConsole(options => {
              options.TimestampFormat = "[HH:mm:ss] ";
              options.IncludeScopes = false;
              });
        })
        .ConfigureServices((context, services) => {
            services.AddHostedService<StatusbarService>();

            services.Configure<StatusbarSettings>(
              context.Configuration.GetSection("Statusbar")
                );
        })
        .Build()
        .RunAsync();
}

