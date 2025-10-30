using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using WaffleCLI.SampleApp.Models;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for displaying greeting messages with decorative formatting
/// </summary>
[Command("greet", "Greet a user")]
public class GreetCommand : ICommand
{
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the GreetCommand class
    /// </summary>
    /// <param name="output">Console output service</param>
    public GreetCommand(IConsoleOutput output)
    {
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "greet";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Greet a user";

    /// <summary>
    /// Displays a decorative greeting message
    /// </summary>
    /// <param name="args">Optional name argument for personalized greeting</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var name = args.Length > 0 ? args[0] : new AppSettings().DefaultUser;
        
        _output.WriteLine("╔══════════════════════════════╗", ConsoleColor.Green);
        _output.WriteLine("║           GREETING           ║", ConsoleColor.Green);
        _output.WriteLine("╚══════════════════════════════╝", ConsoleColor.Green);
        _output.WriteLine($"Hello, {name}!", ConsoleColor.Yellow);
        _output.WriteLine();

        return Task.CompletedTask;
    }
}