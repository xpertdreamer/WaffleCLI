using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("help", "Show available commands")]
public class HelpCommand : ICommand
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly IConsoleOutput _output;
    
    public HelpCommand(ICommandRegistry commandRegistry, IConsoleOutput output)
    {
        _commandRegistry = commandRegistry;
        _output = output;
    }

    public string Name => "help";
    public string Description => "Show available commands";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        var commands = _commandRegistry.GetCommands();
        
        _output.WriteLine("Available commands:", ConsoleColor.Yellow);
        _output.WriteLine("===================");

        foreach (var command in commands.OrderBy(c => c.Name))
        {
            _output.WriteLine($"  {command.Name} - {command.Description}", ConsoleColor.Green);
            
            if (command is ICommandGroup group)
            {
                foreach (var subCommand in group.SubCommands.Values.OrderBy(sc => sc.Name))
                {
                    Console.WriteLine($"    {command.Name} {subCommand.Name} - {subCommand.Description}");
                    
                }
            }
        }

        return Task.CompletedTask;
    }
}