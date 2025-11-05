using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Runtime.Hosting;
using WaffleCLI.SampleApp.Commands;

try
{
    var host = new ConsoleHostBuilder()
        .UseConfiguration("appsettings.json")
        .ConfigureFromOptions()
        .ConfigureServices((_, services) =>
        {
            services.AddWaffleCli();

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