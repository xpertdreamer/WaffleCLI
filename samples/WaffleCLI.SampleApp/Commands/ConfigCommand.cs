using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("config", "Show application configuration")]
public class ConfigCommand : ICommand
{
    private readonly AppSettings _settings;
    private readonly IConsoleOutput _output;

    public ConfigCommand(AppSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    public string Name => "config";
    public string Description => "Show application configuration";

    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _output.WriteLine("Application Configuration:", ConsoleColor.Cyan);
        _output.WriteLine("==========================", ConsoleColor.Cyan);
        _output.WriteLine($"AppName: {_settings.AppName}");
        _output.WriteLine($"Version: {_settings.Version}");
        _output.WriteLine($"DefaultUser: {_settings.DefaultUser}");
        _output.WriteLine($"MaxConnections: {_settings.MaxConnections}");
        _output.WriteLine($"Logging: {(_settings.EnableFeatures.Logging ? "Enabled" : "Disabled")}");
        _output.WriteLine($"Metrics: {(_settings.EnableFeatures.Metrics ? "Enabled" : "Disabled")}");
        _output.WriteLine($"AutoUpdate: {(_settings.EnableFeatures.AutoUpdate ? "Enabled" : "Disabled")}");

        return Task.CompletedTask;
    }
}