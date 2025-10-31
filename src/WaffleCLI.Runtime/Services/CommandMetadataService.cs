using System.Reflection;
using System.Windows.Input;
using WaffleCLI.Core.Models;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Service for extracting and managing command metadata
/// </summary>
public class CommandMetadataService
{
    /// <summary>
    /// Gets metadata for all commands in the specified assemblies
    /// </summary>
    public IEnumerable<CommandMetadata> GetAllCommandsMetadata(IEnumerable<Assembly> assemblies)
    {
        var commands = new List<CommandMetadata>();

        foreach (var assembly in assemblies)
        {
            commands.AddRange(GetCommandsFromAssembly(assembly));
        }

        return commands;
    }

    private IEnumerable<CommandMetadata> GetCommandsFromAssembly(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var commandType in commandTypes) 
        {
            var attribute = commandType.GetCustomAttribute<CommandAttribute>();
            if (attribute != null)
            {
                yield return new CommandMetadata
                {
                    Name = attribute.Name,
                    Description = attribute.Description,
                    Usage = attribute.Usage,
                    Aliases = attribute.Aliases,
                    IsHidden = attribute.IsHidden,
                    CommandType = commandType,
                    Category = attribute.Category
                };
            }
        }
    }

    /// <summary>
    /// Gets metadata for a specific command type
    /// </summary>
    public CommandMetadata GetCommandMetadata(Type commandType)
    {
        var attribute = commandType.GetCustomAttribute<CommandAttribute>();
        if (attribute != null)
        {
            return new CommandMetadata
            {
                Name = attribute.Name,
                Description = attribute.Description,
                Usage = attribute.Usage,
                Aliases = attribute.Aliases,
                IsHidden = attribute.IsHidden,
                CommandType = commandType,
                Category = attribute.Category
            };
        }
        throw new InvalidOperationException($"Command type {commandType.Name} does not have a CommandAttribute");
    }
    
    /// <summary>
    /// Gets parameter metadata for a command
    /// </summary>
    public IEnumerable<ParameterInfo> GetParameterMetadata(Type commandType)
    {
        return commandType.GetProperties()
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() != null)
            .Select(p => new ParameterInfo
            {
                Property = p,
                Attribute = p.GetCustomAttribute<ParameterAttribute>()!
            });
    }
}

/// <summary>
/// Represents parameter information
/// </summary>
public class ParameterInfo
{
    /// <summary>
    /// Gets or sets property info
    /// </summary>
    public PropertyInfo Property { get; set; } = null!;
    /// <summary>
    /// Gets or sets parameter attribute
    /// </summary>
    public ParameterAttribute Attribute { get; set; } = null!;
}