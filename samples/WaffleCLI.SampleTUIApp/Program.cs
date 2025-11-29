using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Screens;
using WaffleCLI.Runtime.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddWaffleTui(tuiApplicationBuilder =>
    {
        tuiApplicationBuilder.UseStartScreen<ProcessManagerScreen>();
    });
});

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Warning);
});

var host = builder.Build();

try
{
    var app = host.Services.GetRequiredService<ITuiApplication>();
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    var service = host.Services.GetService<ITuiApplication>();
    if (service == null)
    {
        Console.WriteLine("ITuiApplication is not registered.");
    }
    else
    {
        Console.WriteLine($"ITuiApplication is registered as {service.GetType().FullName}");
    }
    return 1;
}

return 0;