using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.SampleApp.Commands;

[Command("help", "Show available commands")]
public class HelpCommand : ICommand
{
    private readonly ICommandRegistry _commandRegistry;

    public HelpCommand(ICommandRegistry commandRegistry)
    {
        _commandRegistry = commandRegistry;
    }

    public string Name => "help";
    public string Description => "Show available commands";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        var commands = _commandRegistry.GetCommands();

        Console.WriteLine("Available commands:");
        Console.WriteLine("===================");

        foreach (var command in commands.OrderBy(c => c.Name))
        {
            Console.WriteLine($"  {command.Name} - {command.Description}");
            
            // Если это CommandGroup, показываем подкоманды
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