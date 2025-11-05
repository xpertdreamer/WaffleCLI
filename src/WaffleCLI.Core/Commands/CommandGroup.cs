using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.Core.Commands;

/// <summary>
/// Provides a base implementation for command groups that can contain and manage subcommands.
/// </summary>
/// <remarks>
/// This abstract class handles subcommand discovery, execution routing, and help system generation
/// for hierarchical CLI command structures.
/// </remarks>
public abstract class CommandGroup : ICommandGroup
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsoleOutput _output;
    private Dictionary<string, ICommand>? _subCommands;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGroup"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used for resolving subcommand dependencies.</param>
    /// <param name="output">The console output service for writing messages and errors.</param>
    protected CommandGroup(IServiceProvider serviceProvider, IConsoleOutput output)
    {
        _serviceProvider = serviceProvider;
        _output = output;
    }

    /// <summary>
    /// Gets the name of the command group.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the description of the command group.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the read-only dictionary of discovered subcommands, keyed by their command name.
    /// </summary>
    public IReadOnlyDictionary<string, ICommand> SubCommands => 
        _subCommands ??= DiscoverSubCommands();

    /// <summary>
    /// Executes the command group with the specified arguments.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the command group.</param>
    /// <param name="token">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous execution operation.</returns>
    /// <remarks>
    /// If no arguments are provided or the first argument is "help", displays the help text.
    /// Otherwise, routes execution to the appropriate subcommand.
    /// </remarks>
    public virtual Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length == 0 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            ShowHelp();
            return Task.CompletedTask;
        }

        var subCommandName = args[0];
        if (SubCommands.TryGetValue(subCommandName, out var command))
        {
            return command.ExecuteAsync(args[1..], token);
        }

        _output.WriteError($"Unknown subcommand: {subCommandName}");
        ShowHelp();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the formatted help text for this command group and its subcommands.
    /// </summary>
    /// <returns>A string containing the formatted help text.</returns>
    public virtual string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Name} - {Description}");
        sb.AppendLine();
        sb.AppendLine("Available subcommands:");
        sb.AppendLine();
        
        foreach (var (name, command) in SubCommands.OrderBy(x => x.Key))
        {
            sb.AppendLine($"  {name} - {command.Description}");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Displays the help information for this command group to the console output.
    /// </summary>
    /// <remarks>
    /// Uses colored output to enhance readability in terminal environments.
    /// </remarks>
    protected virtual void ShowHelp()
    {
        _output.WriteLine($"{Name} - {Description}", ConsoleColor.Cyan);
        _output.WriteLine("Available subcommands:", ConsoleColor.Yellow);
        _output.WriteLine();
        
        foreach (var (name, command) in SubCommands.OrderBy(x => x.Key))
        {
            _output.Write($"  {name}", ConsoleColor.Green);
            _output.WriteLine($" - {command.Description}");
        }
        
        _output.WriteLine();
        _output.WriteLine($"Use '{Name} <subcommand> --help' for more information.", ConsoleColor.DarkGray);
    }

    /// <summary>
    /// Discovers and instantiates all subcommands that belong to this command group.
    /// </summary>
    /// <returns>A dictionary of subcommands keyed by their command name.</returns>
    /// <remarks>
    /// Uses reflection to find all types marked with <see cref="SubCommandAttribute"/> that reference this group.
    /// Handles assembly loading errors gracefully and reports failures through the output service.
    /// </remarks>
    private Dictionary<string, ICommand> DiscoverSubCommands()
    {
        var commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
        
        var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => 
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return Enumerable.Empty<Type>();
                }
            })
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && 
                          !type.IsAbstract &&
                          type.GetCustomAttribute<SubCommandAttribute>()?.ParentGroup == Name);

        foreach (var commandType in commandTypes)
        {
            try
            {
                var command = (ICommand)_serviceProvider.GetRequiredService(commandType);
                var attribute = commandType.GetCustomAttribute<SubCommandAttribute>();
                var commandName = attribute?.Name ?? command.Name.ToLowerInvariant();
                
                if (!commands.ContainsKey(commandName))
                {
                    commands[commandName] = command;
                }
            }
            catch (Exception ex)
            {
                _output.WriteError($"Failed to load subcommand {commandType.Name}: {ex.Message}");
            }
        }

        return commands;
    }
}