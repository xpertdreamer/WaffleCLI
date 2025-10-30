using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Provides command registration and resolution functionality for the WaffleCLI application.
/// Maintains a registry of available commands and facilitates their instantiation through dependency injection.
/// </summary>
public class CommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, Type> _commandTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRegistry> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRegistry"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used for resolving command dependencies.</param>
    /// <param name="logger">The logger for recording command registration and resolution events.</param>
    public CommandRegistry(IServiceProvider serviceProvider, ILogger<CommandRegistry> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Registers a command type with the registry using the command name derived from its <see cref="CommandAttribute"/>
    /// or by convention from the type name.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to register, must implement <see cref="ICommand"/>.</typeparam>
    /// <exception cref="ArgumentException">Thrown when the type does not implement <see cref="ICommand"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a command with the same name is already registered.</exception>
    public void RegisterCommand<TCommand>() where TCommand : ICommand
    {
        RegisterCommand(typeof(TCommand));
    }

    /// <summary>
    /// Registers a command type with the registry using the command name derived from its <see cref="CommandAttribute"/>
    /// or by convention from the type name.
    /// </summary>
    /// <param name="commandType">The type of command to register.</param>
    /// <exception cref="ArgumentException">Thrown when the type does not implement <see cref="ICommand"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a command with the same name is already registered.</exception>
    public void RegisterCommand(Type commandType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Type {commandType.FullName} is not a command");
        }
        
        var attribute = commandType.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;
        var commandName = attribute?.Name ??  commandType.Name.Replace("Command", "").ToLower();

        if (_commandTypes.ContainsKey(commandName))
        {
            throw new InvalidOperationException($"Command '{commandName}' is already registered");
        }
        
        _commandTypes[commandName] = commandType;
        _logger.LogDebug("Registered command {CommandName} -> {CommandType}", commandName, commandType.Name);
    }

    /// <summary>
    /// Retrieves a command instance by name using the dependency injection container.
    /// </summary>
    /// <param name="name">The name of the command to retrieve.</param>
    /// <returns>
    /// An instance of the requested command if found and successfully created; otherwise, <c>null</c>.
    /// </returns>
    public ICommand? GetCommand(string name)
    {
        if (!_commandTypes.TryGetValue(name, out var commandType)) return null;
        try
        {
            return (ICommand)_serviceProvider.GetRequiredService(commandType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create command instance {CommandType}", commandType.Name);
            return null;
        }
    }

    /// <summary>
    /// Retrieves all registered command instances.
    /// </summary>
    /// <returns>
    /// A read-only collection of all successfully instantiated commands.
    /// Commands that fail to instantiate are silently skipped.
    /// </returns>
    public IReadOnlyCollection<ICommand> GetCommands()
    {
        var commands = new List<ICommand>();
        foreach (var commandType in _commandTypes.Values)
        {
            var command = GetCommand(GetCommandName(commandType));
            if (command != null)
            {
                commands.Add(command);
            }
        }

        return commands.AsReadOnly();
    }

    /// <summary>
    /// Derives the command name from a command type using its <see cref="CommandAttribute"/>
    /// or by convention from the type name.
    /// </summary>
    /// <param name="commandType">The command type to derive the name from.</param>
    /// <returns>The command name.</returns>
    private static string GetCommandName(Type commandType)
    {
        var attribute = commandType.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;
        return attribute?.Name ?? commandType.Name.Replace("Command", "").ToLower();
    }
}