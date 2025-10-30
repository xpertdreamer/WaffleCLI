using Microsoft.Extensions.Configuration;
using WaffleCLI.Hosting.Extensions;
using WaffleCLI.Hosting;
using Microsoft.Extensions.Logging;
using WaffleCLI.SampleApp.Commands;
using WaffleCLI.SampleApp.Models;

try
{
    var host = new ConsoleHostBuilder()
        .ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true);
            config.AddEnvironmentVariables("CONSOLE_");
        })
        .ConfigureServices((context, services) =>
        {
            // Register commands in DI container
            services.AddCommand<HelpCommand>();
            services.AddCommand<ExitCommand>();
            services.AddCommand<GreetCommand>();
            services.AddCommand<CalcCommand>();
            services.AddCommand<FileManagerCommand>();
            services.AddCommand<ConfigCommand>();
            services.AddCommand<SystemInfoCommand>();
            services.AddCommand<DatabaseCommand>();
            services.AddCommand<WeatherCommand>();
            
            // Register configuration sections
            services.ConfigureSection<AppSettings>("AppSettings");
            services.ConfigureSection<DatabaseSettings>("Database");
            services.ConfigureSection<WeatherSettings>("Weather");
        })
        .ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        })
        .UseConsoleLifetime()
        .Build();

    return await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex}");
    return 1;
}