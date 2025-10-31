using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Hosting.Extensions;
using WaffleCLI.Hosting;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Scripting;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Runtime.Scripting;
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

            // Register command groups in DI container
            services.AddCommand<FileCommandGroup>();

            // Register middleware in DI container
            services.AddTransient<ICommandMiddleware, LoggingMiddleware>();
            services.AddTransient<ICommandMiddleware, TimingMiddleware>();
            services.AddTransient<ICommandMiddleware, ValidationMiddleware>();
            services.AddTransient<ICommandMiddleware, ExceptionHandlingMiddleware>();
            
            // Register configuration sections
            services.ConfigureSection<AppSettings>("AppSettings");
            services.ConfigureSection<DatabaseSettings>("Database");
            services.ConfigureSection<WeatherSettings>("Weather");
            
            services.AddSingleton<IScriptEngine, ScriptEngine>();
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