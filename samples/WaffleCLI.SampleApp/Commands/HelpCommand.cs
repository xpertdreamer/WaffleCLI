using Spectre.Console;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.SampleApp.Commands;

[Command("help", "Show available commands")]
public class HelpCommand : ICommand
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly IAnsiConsoleOutput _output;

    public HelpCommand(ICommandRegistry commandRegistry, IAnsiConsoleOutput output)
    {
        _commandRegistry = commandRegistry;
        _output = output;
    }
    
    public string Name => "help";
    public string Description => "Show available commands";

    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var commands = _commandRegistry.GetCommands();

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.Title = new TableTitle("available commands");

        table.AddColumn("Command");
        table.AddColumn("Description");

        foreach (var com in commands.OrderBy(c => c.Name))
        {
            table.AddRow(
                $"[green]{com.Name}[/]",
                com.Description
            );
        }
        
        AnsiConsole.Write(table);
        return Task.CompletedTask;
    }
}