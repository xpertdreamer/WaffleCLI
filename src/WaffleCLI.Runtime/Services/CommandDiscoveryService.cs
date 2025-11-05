using System.Reflection;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Configuration;
using Microsoft.Extensions.Logging;
using WaffleCLI.Core.Commands;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Service responsible for discovering and categorizing command types from assemblies using reflection.
/// </summary>
/// <remarks>
/// Scans assemblies for types implementing <see cref="ICommand"/> and categorizes them based on attributes
/// and naming conventions. Handles assembly loading errors gracefully and provides detailed logging.
/// </remarks>
public class CommandDiscoveryService
{
    private readonly ILogger<CommandDiscoveryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandDiscoveryService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for recording discovery process and errors.</param>
    public CommandDiscoveryService(ILogger<CommandDiscoveryService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Discovers and categorizes command types from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for command types.</param>
    /// <param name="options">The registration options for configuring discovery behavior.</param>
    /// <returns>A <see cref="CommandDiscoveryResult"/> containing categorized command types.</returns>
    /// <remarks>
    /// Processes each assembly sequentially, catching and logging any assembly scanning errors
    /// without stopping the discovery process for other assemblies. Applies naming convention
    /// discovery if enabled in the options.
    /// </remarks>
    public CommandDiscoveryResult DiscoverCommands(IEnumerable<Assembly> assemblies, CommandRegistrationOptions? options = null)
    {
        options ??= new CommandRegistrationOptions();
        var result = new CommandDiscoveryResult();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                _logger.LogDebug("Scanning assembly {Assembly} for commands", assembly.GetName().Name);
                
                var types = assembly.GetTypes()
                    .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();

                foreach (var type in types)
                {
                    DiscoverCommandType(type, result, options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan assembly {Assembly}", assembly.GetName().Name);
            }
        }

        _logger.LogInformation("Discovered {Standalone} standalone commands, {Groups} groups, {SubCommands} subcommands",
            result.StandaloneCommands.Count, result.CommandGroups.Count, result.SubCommands.Count);

        return result;
    }

    /// <summary>
    /// Categorizes a single command type based on its attributes and naming conventions.
    /// </summary>
    /// <param name="type">The command type to categorize.</param>
    /// <param name="result">The discovery result to update with the categorized type.</param>
    /// <param name="options">The registration options for configuring discovery behavior.</param>
    /// <remarks>
    /// Checks for <see cref="CommandGroupAttribute"/>, <see cref="SubCommandAttribute"/>, and <see cref="CommandAttribute"/>
    /// in that order. If no attributes are found and naming conventions are enabled, falls back to naming convention detection.
    /// </remarks>
    private void DiscoverCommandType(Type type, CommandDiscoveryResult result, CommandRegistrationOptions options)
    {
        var commandAttr = type.GetCustomAttribute<CommandAttribute>();
        var commandGroupAttr = type.GetCustomAttribute<CommandGroupAttribute>();
        var subCommandAttr = type.GetCustomAttribute<SubCommandAttribute>();

        if (commandGroupAttr != null)
        {
            result.CommandGroups.Add(type);
            _logger.LogDebug("Discovered command group: {Name} -> {Type}", commandGroupAttr.Name, type.Name);
        }
        else if (subCommandAttr != null)
        {
            result.SubCommands.Add((type, subCommandAttr.ParentGroup));
            _logger.LogDebug("Discovered subcommand: {Parent}.{Command} -> {Type}", 
                subCommandAttr.ParentGroup, subCommandAttr.Name ?? DeriveCommandName(type), type.Name);
        }
        else if (commandAttr != null)
        {
            result.StandaloneCommands.Add(type);
            _logger.LogDebug("Discovered standalone command: {Name} -> {Type}", commandAttr.Name, type.Name);
        }
        // Automatic discovery by naming convention if enabled
        else if (options.UseNamingConvention && type.Name.EndsWith("Command") && !typeof(CommandGroup).IsAssignableFrom(type))
        {
            var autoName = DeriveCommandName(type);
            result.StandaloneCommands.Add(type);
            _logger.LogDebug("Auto-discovered command: {Name} -> {Type}", autoName, type.Name);
        }
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

/// <summary>
/// Represents the result of a command discovery operation, containing categorized command types.
/// </summary>
public class CommandDiscoveryResult
{
    /// <summary>
    /// Gets the list of discovered standalone command types.
    /// </summary>
    /// <remarks>
    /// These are commands that are not part of a group and can be executed directly.
    /// </remarks>
    public List<Type> StandaloneCommands { get; } = new();
    
    /// <summary>
    /// Gets the list of discovered command group types.
    /// </summary>
    /// <remarks>
    /// These are types that can contain and manage subcommands in a hierarchical structure.
    /// </remarks>
    public List<Type> CommandGroups { get; } = new();
    
    /// <summary>
    /// Gets the list of discovered subcommand types with their parent group information.
    /// </summary>
    /// <remarks>
    /// Each tuple contains the subcommand type and the name of its parent command group.
    /// </remarks>
    public List<(Type CommandType, string ParentGroup)> SubCommands { get; } = new();
    
    /// <summary>
    /// Gets all discovered command types combined into a single enumerable.
    /// </summary>
    /// <remarks>
    /// Provides a convenient way to iterate over all command types regardless of their category.
    /// </remarks>
    public IEnumerable<Type> AllCommandTypes => 
        StandaloneCommands.Concat(CommandGroups).Concat(SubCommands.Select(x => x.CommandType));
}