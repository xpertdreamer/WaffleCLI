using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Service for managing command registration and discovery
/// </summary>
public class CommandRegistrationService
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly IServiceCollection _services;
    private readonly ILogger<CommandRegistrationService> _logger;

    public CommandRegistrationService(ICommandRegistry commandRegistry, IServiceCollection services,
        ILogger<CommandRegistrationService> logger)
    {
        _commandRegistry = commandRegistry;
        _services = services;
        _logger = logger;
    }

    /// <summary>
    /// Automatically discovers and registers all commands from assemblies
    /// </summary>
    public void AutoRegisterCommands(IEnumerable<Assembly> assemblies)
    {
        var allCommandTypes = new List<Type>();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var commandTypes = assembly.GetTypes()
                    .Where(t => typeof(ICommand).IsAssignableFrom(t) && 
                                !t.IsAbstract && 
                                !t.IsInterface)
                    .ToList();

                allCommandTypes.AddRange(commandTypes);
                _logger.LogDebug("Found {Count} command types in assembly {Assembly}", 
                    commandTypes.Count, assembly.GetName().Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan assembly {Assembly} for commands", 
                    assembly.GetName().Name);
            }
        }

        // Separate commands into groups and standalone commands
        var (standaloneCommands, subCommands) = CategorizeCommands(allCommandTypes);

        // Register standalone commands
        RegisterStandaloneCommands(standaloneCommands);

        // Register subcommands
        RegisterSubCommands(subCommands);

        _logger.LogInformation("Auto-registered {StandaloneCount} standalone commands and {SubCommandCount} subcommands", 
            standaloneCommands.Count, subCommands.Count);
    }
    
    /// <summary>
    /// Categorizes commands into standalone commands and subcommands
    /// </summary>
    private (List<Type> standaloneCommands, Dictionary<string, List<Type>> subCommands) 
        CategorizeCommands(List<Type> commandTypes)
    {
        var standaloneCommands = new List<Type>();
        var subCommands = new Dictionary<string, List<Type>>();

        foreach (var commandType in commandTypes)
        {
            // Check if it's a subcommand
            var subCommandAttr = commandType.GetCustomAttribute<SubCommandAttribute>();
            if (subCommandAttr != null)
            {
                var parentGroup = subCommandAttr.ParentGroup;
                if (!subCommands.ContainsKey(parentGroup))
                {
                    subCommands[parentGroup] = new List<Type>();
                }
                subCommands[parentGroup].Add(commandType);
            }
            // Check if it's a command group
            else if (typeof(CommandGroup).IsAssignableFrom(commandType))
            {
                standaloneCommands.Add(commandType);
            }
            // Check if it's a standalone command with CommandAttribute
            else if (commandType.GetCustomAttribute<CommandAttribute>() != null)
            {
                standaloneCommands.Add(commandType);
            }
            else
            {
                _logger.LogDebug("Skipping type {TypeName} - no CommandAttribute or SubCommandAttribute found", 
                    commandType.Name);
            }
        }

        return (standaloneCommands, subCommands);
    }

    /// <summary>
    /// Registers standalone commands in the registry and DI container
    /// </summary>
    private void RegisterStandaloneCommands(List<Type> standaloneCommands)
    {
        foreach (var commandType in standaloneCommands)
        {
            try
            {
                // Register in DI container
                _services.AddTransient(commandType);
                
                // Register in command registry
                _commandRegistry.RegisterCommand(commandType);
                
                _logger.LogDebug("Registered standalone command: {CommandType}", commandType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to register standalone command {CommandType}", commandType.Name);
            }
        }
    }

    /// <summary>
    /// Registers subcommands in the DI container (they will be picked up by command groups)
    /// </summary>
    private void RegisterSubCommands(Dictionary<string, List<Type>> subCommands)
    {
        foreach (var (parentGroup, subCommandTypes) in subCommands)
        {
            foreach (var subCommandType in subCommandTypes)
            {
                try
                {
                    // Register subcommand in DI container
                    _services.AddTransient(subCommandType);
                    
                    _logger.LogDebug("Registered subcommand {SubCommand} for group {ParentGroup}", 
                        subCommandType.Name, parentGroup);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to register subcommand {SubCommand} for group {ParentGroup}", 
                        subCommandType.Name, parentGroup);
                }
            }
        }
    }

    /// <summary>
    /// Gets all command groups that should be registered
    /// </summary>
    public List<Type> GetCommandGroupTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(assembly => 
            {
                try
                {
                    return assembly.GetTypes()
                        .Where(t => typeof(CommandGroup).IsAssignableFrom(t) && 
                                   !t.IsAbstract);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to get types from assembly {Assembly}", assembly.GetName().Name);
                    return Enumerable.Empty<Type>();
                }
            })
            .ToList();
    }
}