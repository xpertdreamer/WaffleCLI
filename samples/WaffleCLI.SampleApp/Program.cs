using Microsoft.Extensions.Configuration;
using WaffleCLI.Hosting.Extensions;
using WaffleCLI.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.SampleApp;
using WaffleCLI.SampleApp.Commands;

var host = new ConsoleHostBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true);
        config.AddEnvironmentVariables("CONSOLE_");
    })
    .ConfigureServices((context, services) =>
    {
        // Register commands
        services.AddCommand<HelpCommand>();
        // services.AddCommand<ExitCommand>();
        // services.AddCommand<GreetCommand>();
        // services.AddCommand<CalcCommand>();
        // services.AddCommand<FileManagerCommand>();
        // services.AddCommand<ConfigCommand>();
        // services.AddCommand<SystemInfoCommand>();
        // services.AddCommand<DatabaseCommand>();
        // services.AddCommand<WeatherCommand>();
        
        // Register configuration sections
        services.Configure<AppSettings>(
            context.Configuration.GetSection("AppSettings"));
        // services.Configure<DatabaseSettings>(
        //     context.Configuration.GetSection("Database"));
        // services.Configure<WeatherSettings>(
        //     context.Configuration.GetSection("Weather"));
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .UseConsoleLifetime()
    .Build();

return await host.RunAsync();

