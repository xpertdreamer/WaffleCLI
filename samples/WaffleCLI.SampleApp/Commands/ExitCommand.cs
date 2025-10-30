using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("exit", "Exit the application")]
public class ExitCommand : ICommand
{
    private readonly IConsoleHost _host;
    private readonly IConsoleOutput _output;

    public ExitCommand(IConsoleHost host, IConsoleOutput output)
    {
        _host = host;
        _output = output;
    }

    public string Name => "exit";
    public string Description => "Exit the application";

    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _output.WriteInfo("Goodbye! Closing application...");
        return Task.CompletedTask;
    }
}