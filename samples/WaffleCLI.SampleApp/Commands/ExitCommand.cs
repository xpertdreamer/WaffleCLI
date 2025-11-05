using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.SampleApp.Commands;

[Command("exit", "Exit the application")]
public class ExitCommand : ICommand
{
    public string Name => "exit";
    public string Description => "Exit the application";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}