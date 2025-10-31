using System.Text;
using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Output;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Provides a base implementation for command groups that contain multiple subcommands.
/// Command groups organize related commands under a common namespace and handle routing to subcommands.
/// </summary>
/// <remarks>
/// <para>
/// Command groups are used to create hierarchical command structures where a parent command
/// acts as a container for multiple related subcommands. This pattern is commonly used in
/// CLI applications to organize functionality (e.g., 'git push', 'git pull', 'git commit').
/// </para>
/// <para>
/// Inherit from this class to create custom command groups and register subcommands using
/// the <see cref="RegisterSubCommand"/> method, typically in the constructor.
/// </para>
/// </remarks>
public abstract class CommandGroup : ICommandGroup
{
    private readonly Dictionary<string, Type> _subCommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGroup"/> class.
    /// </summary>
    /// <param name="output">The console output service for displaying messages and help text.</param>
    /// <param name="provider">The console service provider</param>
    protected CommandGroup(IConsoleOutput output, IServiceProvider provider)
    {
        _serviceProvider =  provider;
        _output = output;
    }

    /// <summary>
    /// Gets the name of the command group used to invoke it from the command line.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the description of the command group displayed in help text.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets a read-only dictionary of registered subcommands available in this command group.
    /// </summary>
    public IReadOnlyDictionary<string, ICommand> SubCommands => GetSubCommands();

    /// <summary>
    /// Registers a subcommand with the command group.
    /// </summary>
    /// <param name="name">The name of the subcommand used to invoke it.</param>
    /// <exception cref="ArgumentException">Thrown when a subcommand with the same name is already registered.</exception>
    /// <remarks>
    /// Subcommand names are case-insensitive. Typically, subcommands are registered in the
    /// constructor of the command group implementation.
    /// </remarks>
    public void RegisterSubCommand<TSubCommand>(string name) where TSubCommand : ICommand
    {
        _subCommands[name] = typeof(TSubCommand);
    }

    /// <summary>
    /// Executes the command group by routing to the appropriate subcommand based on the provided arguments.
    /// </summary>
    /// <param name="args">The command arguments. The first argument should be the subcommand name.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>A task that represents the asynchronous command execution.</returns>
    /// <remarks>
    /// <para>
    /// If no arguments are provided or the first argument is "help", the help text for the command group is displayed.
    /// </para>
    /// <para>
    /// If a valid subcommand name is provided, the execution is delegated to that subcommand with the remaining arguments.
    /// If the subcommand is not found, an error message is displayed followed by the help text.
    /// </para>
    /// </remarks>
    public virtual Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length == 0 || args[0] == "help")
        {
            ShowHelp();
            return Task.CompletedTask;
        }
        
        var subCommandName = args[0];
        if (_subCommands.TryGetValue(subCommandName, out var commandType))
        {
            try
            {
                var command = (ICommand)_serviceProvider.GetRequiredService(commandType);
                return command.ExecuteAsync(args[1..], token);
            }
            catch (Exception ex)
            {
                _output.WriteError($"Failed to create subcommand '{subCommandName}': {ex.Message}");
                return Task.CompletedTask;
            }
        }
        
        _output.WriteError($"Unknown subcommand: {subCommandName}");
        ShowHelp();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Displays formatted help text for the command group to the console output.
    /// </summary>
    /// <remarks>
    /// The help text includes the command group name, description, and a list of available
    /// subcommands with their descriptions. The output is colorized for better readability.
    /// </remarks>
    protected virtual void ShowHelp()
    {
        _output.WriteLine($"{Name} - {Description}", ConsoleColor.Cyan);
        _output.WriteLine("Available subcommands:", ConsoleColor.Yellow);
        _output.WriteLine();
        
        foreach (var subCommand in _subCommands.OrderBy(sc => sc.Key))
        {
            try
            {
                var commandInstance = (ICommand)_serviceProvider.GetRequiredService(subCommand.Value);
                
                _output.Write($"  {subCommand.Key}", ConsoleColor.Green);
                _output.WriteLine($" - {commandInstance.Description}");
            }
            catch (Exception ex)
            {
                _output.Write($"  {subCommand.Key}", ConsoleColor.Red);
                _output.WriteLine($" - [ERROR: {ex.Message}]");
            }
        }
        
        _output.WriteLine();
        _output.WriteLine($"Use '{Name} <subcommand> --help' for more information about a subcommand.", ConsoleColor.DarkGray);
    }
    
    /// <summary>
    /// Gets the plain text help documentation for the command group.
    /// </summary>
    /// <returns>A string containing the formatted help text without color information.</returns>
    /// <remarks>
    /// This method is useful for generating help text in contexts where console colors are not available,
    /// such as in documentation or when exporting help information to files.
    /// </remarks>
    public virtual string GetHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Name} - {Description}");
        sb.AppendLine();
        sb.AppendLine("Available subcommands:");
        sb.AppendLine();
        
        foreach (var subCommand in _subCommands.OrderBy(sc => sc.Key))
        {
            try
            {
                var commandInstance = (ICommand)_serviceProvider.GetRequiredService(subCommand.Value);
                sb.AppendLine($"  {subCommand.Key} - {commandInstance.Description}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  {subCommand.Key} - [ERROR: {ex.Message}]");
            }
        }
        
        return sb.ToString();
    }

    private IReadOnlyDictionary<string, ICommand> GetSubCommands()
    {
        var commands = new Dictionary<string, ICommand>();

        foreach (var subCommand in _subCommands)
        {
            try
            {
                var commandInstance = (ICommand)_serviceProvider.GetRequiredService(subCommand.Value.GetType());
                commands[subCommand.Key] = commandInstance;
            }
            catch
            {
                // Ignore 
            }
        }
        return commands;
    }
}