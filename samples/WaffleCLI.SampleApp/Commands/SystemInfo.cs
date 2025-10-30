using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("sysinfo", "Show system information")]
public class SystemInfoCommand : ICommand
{
    private readonly AppSettings _settings;
    private readonly IConsoleOutput _output;

    public SystemInfoCommand(AppSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    public string Name => "sysinfo";
    public string Description => "Show system information";

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