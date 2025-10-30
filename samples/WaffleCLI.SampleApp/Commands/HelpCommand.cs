using System.Drawing;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("help", "Show available commands")]
public abstract class HelpCommand : ICommand
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly IConsoleOutput _output;

    protected HelpCommand(ICommandRegistry commandRegistry, IConsoleOutput output)
    {
        _commandRegistry = commandRegistry;
        _output = output;
    }
    
    public string Name => "help";
    public string Description => "Show available commands";

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