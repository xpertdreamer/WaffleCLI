using System.Reflection;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

[Command("help", "Show available commands", Aliases = ["h", "?"])]
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
        _output.WriteLine();

        var standalone = commands.Where(c => c is not ICommandGroup).OrderBy(c => c.Name);
        var groupCommands = commands.OfType<ICommandGroup>().OrderBy(c => c.Name);
        
        if (standalone.Any())
        {
            _output.WriteLine("Standalone commands:");
            foreach (var command in standalone)
            {
                var aliases = GetCommandAliases(command);
                var aliasText = aliases.Any() ? $" (aliases: {string.Join(", ", aliases)})" : "";
                _output.WriteLine($"  {command.Name} - {command.Description}{aliasText}");
            }
            _output.WriteLine();
        }
        
        if (groupCommands.Any())
        {
            Console.WriteLine("Command groups:");
            foreach (var group in groupCommands)
            {
                _output.WriteLine($"  {group.Name} - {group.Description}");
                
                foreach (var subCommand in group.SubCommands.Values.OrderBy(sc => sc.Name))
                {
                    var subAliases = GetCommandAliases(subCommand);
                    var subAliasText = subAliases.Any() ? $" (aliases: {string.Join(", ", subAliases)})" : "";
                    _output.WriteLine($"    {group.Name} {subCommand.Name} - {subCommand.Description}{subAliasText}");
                }
                _output.WriteLine();
            }
        }
        _output.WriteLine("Use '<command> --help' for more information about a command.");

        return Task.CompletedTask;
    }
    
    private IEnumerable<string> GetCommandAliases(ICommand command)
    {
        var commandType = command.GetType();
        var attribute = commandType.GetCustomAttribute<CommandAttribute>();
        return attribute?.Aliases ?? Array.Empty<string>();
    }
}