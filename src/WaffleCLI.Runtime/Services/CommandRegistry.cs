using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using Microsoft.Extensions.Logging;
using WaffleCLI.Core.Commands;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Maintains a registry of available commands and provides methods for command registration and retrieval.
/// </summary>
/// <remarks>
/// The registry uses case-insensitive command names and supports automatic discovery as well as manual registration.
/// Commands are resolved through the service provider when requested, enabling dependency injection support.
/// </remarks>
public class CommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, Type> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRegistry> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRegistry"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used for resolving command instances.</param>
    /// <param name="logger">The logger for recording registration and resolution events.</param>
    public CommandRegistry(
        IServiceProvider serviceProvider,
        ILogger<CommandRegistry> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the command registry using a pre-discovered command discovery result.
    /// </summary>
    /// <param name="discoveryResult">The discovery result containing categorized command types.</param>
    /// <remarks>
    /// Registers standalone commands, command groups, and logs subcommands for hierarchical command structure.
    /// Provides detailed logging of the initialization process and registered commands.
    /// </remarks>
    public void Initialize(CommandDiscoveryResult discoveryResult)
    {
        try
        {
            _logger.LogInformation("Initializing command registry...");

            RegisterStandaloneCommands(discoveryResult.StandaloneCommands);
            RegisterCommandGroups(discoveryResult.CommandGroups);
            RegisterSubCommands(discoveryResult.SubCommands);

            _logger.LogInformation("Command registry initialized with {Count} commands", _commands.Count);
            
            foreach (var command in _commands.Keys.OrderBy(k => k))
            {
                _logger.LogDebug("Registered command: {Command}", command);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize command registry");
            throw;
        }
    }

    /// <summary>
    /// Registers standalone command types that are not part of a command group.
    /// </summary>
    /// <param name="commandTypes">The standalone command types to register.</param>
    /// <remarks>
    /// Uses the <see cref="CommandAttribute.Name"/> if specified, otherwise derives the name from the type name.
    /// Also registers any aliases defined in the command attribute.
    /// </remarks>
    private void RegisterStandaloneCommands(IEnumerable<Type> commandTypes)
    {
        foreach (var commandType in commandTypes)
        {
            try
            {
                var attribute = commandType.GetCustomAttribute<CommandAttribute>();
                var commandName = attribute?.Name ?? DeriveCommandName(commandType);

                if (_commands.ContainsKey(commandName))
                {
                    _logger.LogWarning("Command '{CommandName}' already registered, skipping", commandName);
                    continue;
                }

                _commands[commandName] = commandType;
                _logger.LogDebug("Registered command: {Name} -> {Type}", commandName, commandType.Name);

                // Register aliases
                if (attribute?.Aliases != null)
                {
                    foreach (var alias in attribute.Aliases)
                    {
                        if (!_commands.ContainsKey(alias))
                        {
                            _commands[alias] = commandType;
                            _logger.LogDebug("Registered alias: {Alias} -> {Type}", alias, commandType.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register command type: {Type}", commandType.Name);
            }
        }
    }

    /// <summary>
    /// Registers command group types that can contain subcommands.
    /// </summary>
    /// <param name="groupTypes">The command group types to register.</param>
    /// <remarks>
    /// Command groups are registered similarly to standalone commands but are responsible for managing their own subcommands.
    /// </remarks>
    private void RegisterCommandGroups(IEnumerable<Type> groupTypes)
    {
        foreach (var groupType in groupTypes)
        {
            try
            {
                var attribute = groupType.GetCustomAttribute<CommandGroupAttribute>();
                var groupName = attribute?.Name ?? DeriveCommandName(groupType);

                if (_commands.ContainsKey(groupName))
                {
                    _logger.LogWarning("Command group '{GroupName}' already registered, skipping", groupName);
                    continue;
                }

                _commands[groupName] = groupType;
                _logger.LogDebug("Registered command group: {Name} -> {Type}", groupName, groupType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register command group: {Type}", groupType.Name);
            }
        }
    }

    /// <summary>
    /// Processes discovered subcommands for logging purposes.
    /// </summary>
    /// <param name="subCommands">The subcommand types with their parent group information.</param>
    /// <remarks>
    /// Subcommands are not directly registered in the main command dictionary as they are managed
    /// by their parent command groups through the <see cref="CommandGroup"/> class.
    /// </remarks>
    private void RegisterSubCommands(IEnumerable<(Type CommandType, string ParentGroup)> subCommands)
    {
        foreach (var (commandType, parentGroup) in subCommands)
        {
            _logger.LogDebug("Discovered subcommand: {Parent}.{Command}", 
                parentGroup, DeriveCommandName(commandType));
        }
    }

    /// <summary>
    /// Manually registers a command type using generics.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to register, must implement <see cref="ICommand"/>.</typeparam>
    /// <remarks>
    /// Provides a type-safe way to register individual commands without using reflection-based discovery.
    /// </remarks>
    public void RegisterCommand<TCommand>() where TCommand : ICommand
    {
        RegisterCommand(typeof(TCommand));
    }

    /// <summary>
    /// Manually registers a command type.
    /// </summary>
    /// <param name="commandType">The type of command to register.</param>
    /// <exception cref="ArgumentException">Thrown when the specified type does not implement <see cref="ICommand"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a command with the same name is already registered.</exception>
    /// <remarks>
    /// Uses the <see cref="CommandAttribute.Name"/> if specified, otherwise derives the name from the type name.
    /// </remarks>
    public void RegisterCommand(Type commandType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Type {commandType.FullName} is not a command");
        }
        
        var attribute = commandType.GetCustomAttribute<CommandAttribute>();
        var commandName = attribute?.Name ?? DeriveCommandName(commandType);

        if (_commands.ContainsKey(commandName))
        {
            throw new InvalidOperationException($"Command '{commandName}' is already registered");
        }
        
        _commands[commandName] = commandType;
        _logger.LogDebug("Manually registered command: {Name} -> {Type}", commandName, commandType.Name);
    }

    /// <summary>
    /// Retrieves a command instance by name.
    /// </summary>
    /// <param name="name">The name of the command to retrieve.</param>
    /// <returns>
    /// An instance of the requested command, or null if the command is not found or cannot be instantiated.
    /// </returns>
    /// <remarks>
    /// The command name lookup is case-insensitive. Command instances are resolved through the service provider,
    /// enabling dependency injection. Returns null if command resolution fails.
    /// </remarks>
    public ICommand? GetCommand(string name)
    {
        if (_commands.TryGetValue(name, out var commandType))
        {
            try
            {
                return (ICommand)_serviceProvider.GetRequiredService(commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create command instance: {Type}", commandType.Name);
                return null;
            }
        }

        _logger.LogDebug("Command not found: {CommandName}", name);
        return null;
    }

    /// <summary>
    /// Retrieves all registered command instances.
    /// </summary>
    /// <returns>A read-only collection of all available command instances.</returns>
    /// <remarks>
    /// Only returns distinct commands (excluding aliases) and filters out commands that cannot be instantiated.
    /// The collection is read-only to prevent external modification of the command registry.
    /// </remarks>
    public IReadOnlyCollection<ICommand> GetCommands()
    {
        var commands = new List<ICommand>();
        foreach (var commandName in _commands.Keys.Distinct())
        {
            var command = GetCommand(commandName);
            if (command != null)
                commands.Add(command);
        }
        return commands.AsReadOnly();
    }

    /// <summary>
    /// Derives a command name from a type name by removing common suffixes and converting to lowercase.
    /// </summary>
    /// <param name="commandType">The command type from which to derive the name.</param>
    /// <returns>The derived command name in lowercase.</returns>
    /// <remarks>
    /// Removes "Command" and "Group" suffixes from the type name and converts the result to lowercase.
    /// For example, "HelpCommand" becomes "help" and "AdminGroup" becomes "admin".
    /// </remarks>
    private static string DeriveCommandName(Type commandType)
    {
        var typeName = commandType.Name;
        if (typeName.EndsWith("Command"))
            typeName = typeName[..^7];
        if (typeName.EndsWith("Group"))  
            typeName = typeName[..^5];
        return typeName.ToLowerInvariant();
    }
}