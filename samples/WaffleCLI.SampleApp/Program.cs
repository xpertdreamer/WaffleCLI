using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Runtime.Hosting;
using WaffleCLI.SampleApp.Commands;

try
{
    var host = new ConsoleHostBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddWaffleCli();
            
            // Automatically register all commands from current assembly
            services.AutoRegisterCommands(Assembly.GetExecutingAssembly());
            
            services.AddTransient<HelpCommand>();
            services.AddTransient<CalcCommand>();
            services.AddTransient<FileCommandGroup>();
            services.AddTransient<FileListCommand>();
            services.AddTransient<FileInfoCommand>();
            services.AddTransient<FileCopyCommand>();
            services.AddTransient<FileDeleteCommand>();
            services.AddTransient<WaffleCommand>();
            services.AddTransient<ExitCommand>();
            services.AddTransient<GreetCommand>();
            
            // Add middleware
            services.AddCommandMiddleware<ExceptionHandlingMiddleware>();
            services.AddCommandMiddleware<TimingMiddleware>();
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
    Console.WriteLine($"Application failed to start: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}