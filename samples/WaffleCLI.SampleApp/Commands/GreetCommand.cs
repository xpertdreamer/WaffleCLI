using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("greet", "Greet a user")]
public class GreetCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public GreetCommand(IConsoleOutput output)
    {
        _output = output;
    }

    public string Name => "greet";
    public string Description => "Greet a user";

    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var name = args.Length > 0 ? args[0] : "Stranger";
        
        _output.WriteLine("╔══════════════════════════════╗", ConsoleColor.Green);
        _output.WriteLine("║           GREETING           ║", ConsoleColor.Green);
        _output.WriteLine("╚══════════════════════════════╝", ConsoleColor.Green);
        _output.WriteLine($"Hello, {name}!", ConsoleColor.Yellow);
        _output.WriteLine();

        return Task.CompletedTask;
    }
}