using WaffleCLI.Hosting;
using WaffleCLI.Hosting.Extensions;
using WaffleCLI.Basic.Commands;

var host = new ConsoleHostBuilder()
    .ConfigureServices(services =>
    {
        // Register your commands here
        services.AddCommand<ExampleCommand>();
        services.AddCommand<HelpCommand>();
    })
    .UseConsoleLifetime()
    .Build();

return await host.RunAsync();