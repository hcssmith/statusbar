using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services;

class Program {
  static async Task Main(string[] args) => await Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) => {
          config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureLogging(logging => {
          logging.AddConsole();
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

