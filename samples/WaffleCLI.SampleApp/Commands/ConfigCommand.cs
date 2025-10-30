using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using WaffleCLI.SampleApp.Models;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for displaying application configuration settings
/// </summary>
[Command("config", "Show application configuration")]
public class ConfigCommand : ICommand
{
    private readonly AppSettings _settings;
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the ConfigCommand class
    /// </summary>
    /// <param name="settings">Application settings</param>
    /// <param name="output">Console output service</param>
    public ConfigCommand(AppSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "config";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Show application configuration";

    /// <summary>
    /// Displays current application configuration
    /// </summary>
    /// <param name="args">Command arguments (not used)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
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