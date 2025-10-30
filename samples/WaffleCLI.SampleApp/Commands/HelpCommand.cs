using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using Microsoft.Extensions.DependencyInjection;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for displaying available commands and their descriptions
/// </summary>
[Command("help", "Show available commands")]
public class HelpCommand : ICommand
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the HelpCommand class
    /// </summary>
    /// <param name="commandRegistry">Command registry service</param>
    /// <param name="output">Console output service</param>
    public HelpCommand(ICommandRegistry commandRegistry, IConsoleOutput output)
    {
        _commandRegistry = commandRegistry;
        _output = output;
    }
    
    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "help";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Show available commands";

    /// <summary>
    /// Displays all available commands with their descriptions
    /// </summary>
    /// <param name="args">Command arguments (not used)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var commands = _commandRegistry.GetCommands();
        
        _output.WriteLine("Available Commands:", ConsoleColor.Cyan);
        _output.WriteLine("===================", ConsoleColor.Cyan);
        _output.WriteLine();
        
        foreach (var command in commands.OrderBy(c => c.Name))
        {
            _output.Write($"  {command.Name}", ConsoleColor.Green);
            _output.WriteLine($" - {command.Description}");
        }

        _output.WriteLine();
        _output.WriteLine("Type 'help <command>' for more information about a command.", ConsoleColor.Yellow);

        return Task.CompletedTask;
    }
}