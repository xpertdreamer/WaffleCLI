using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// 
/// </summary>
internal class CommandRegistry : Abstractions.Commands.ICommandRegistry
{
    private readonly Dictionary<string, Type> _commandTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRegistry> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public CommandRegistry(IServiceProvider serviceProvider, ILogger<CommandRegistry> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public void RegisterCommand<TCommand>() where TCommand : ICommand
    {
        RegisterCommand(typeof(TCommand));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandType"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterCommand(Type commandType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"{commandType.FullName} is not a command");
        }
        
        var attribute = commandType.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;
        var commandName = attribute?.Name ??  commandType.Name.Replace("Command", "").ToLower();

        if (!_commandTypes.ContainsKey(commandName))
        {
            throw new InvalidOperationException($"Command {commandType.FullName} is not registered");
        }
        
        _commandTypes[commandName] = commandType;
        _logger.LogDebug("Registered command {CommandName} -> {CommandType}", commandName, commandType.Name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ICommand? GetCommand(string name)
    {
        if (_commandTypes.TryGetValue(name, out var commandType))
        {
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
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="commandType"></param>
    /// <returns></returns>
    private static string GetCommandName(Type commandType)
    {
        var attribute =
            commandType.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;
        return attribute?.Name ?? commandType.Name.Replace("Command", "").ToLower();
    }
}