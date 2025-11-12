using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Runtime.Hosting;
using WaffleCLI.Runtime.Services;
using WaffleCLI.SampleApp.Commands;

try
{
    var host = new ConsoleHostBuilder()
        .UseConfiguration("appsettings.json")
        .ConfigureServices((_, services) =>
        {
            services.AddCommand<HelpCommand>();
            services.AddCommand<CalcCommand>();
            services.AddCommand<WaffleCommand>();
            services.AddCommand<ExitCommand>();
            services.AddCommand<GreetCommand>();
            services.AddCommand<FileCommandGroup>();
            
            services.AddTransient<FileListCommand>();
            services.AddTransient<FileInfoCommand>();
            services.AddTransient<FileCopyCommand>();
            services.AddTransient<FileDeleteCommand>();

            services.AddSingleton<ICommandRegistry>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<CommandRegistry>>();
                var registry = new CommandRegistry(provider, logger);
                
                registry.RegisterCommand<HelpCommand>();
                registry.RegisterCommand<CalcCommand>();
                registry.RegisterCommand<WaffleCommand>();
                registry.RegisterCommand<ExitCommand>();
                registry.RegisterCommand<GreetCommand>();
                registry.RegisterCommand<FileCommandGroup>();
                
                return registry;
            });
            
            services.AddDefaultMiddleware();
        })
        .UseDefaultLogging()
        .UseConsoleLifetime()
        .Build();

    return await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}