using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using WaffleCLI.SampleApp.Models;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for displaying system and application information
/// </summary>
[Command("sysinfo", "Show system information")]
public class SystemInfoCommand : ICommand
{
    private readonly AppSettings _settings;
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the SystemInfoCommand class
    /// </summary>
    /// <param name="settings">Application settings</param>
    /// <param name="output">Console output service</param>
    public SystemInfoCommand(AppSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "sysinfo";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Show system information";

    /// <summary>
    /// Displays comprehensive system and application information
    /// </summary>
    /// <param name="args">Command arguments (not used)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _output.WriteLine("System Information:", ConsoleColor.Cyan);
        _output.WriteLine("===================", ConsoleColor.Cyan);
        _output.WriteLine($"Application: {_settings.AppName} v{_settings.Version}");
        _output.WriteLine($"OS Version: {Environment.OSVersion}");
        _output.WriteLine($".NET Version: {Environment.Version}");
        _output.WriteLine($"Machine Name: {Environment.MachineName}");
        _output.WriteLine($"User Name: {Environment.UserName}");
        _output.WriteLine($"Processor Count: {Environment.ProcessorCount}");
        _output.WriteLine($"Memory Usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
        _output.WriteLine($"Working Directory: {Environment.CurrentDirectory}");

        return Task.CompletedTask;
    }
}