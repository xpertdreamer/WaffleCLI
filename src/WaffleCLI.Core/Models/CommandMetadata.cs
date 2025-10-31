namespace WaffleCLI.Core.Models;

/// <summary>
/// Enhanced command attribute with additional metadata for registering and describing CLI commands
/// Provides comprehensive information about command name, description, usage, aliases, visibility, and categorization
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets the command name - the primary identifier used to invoke the command from CLI
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the command description - brief explanation of what the command does
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets the command usage examples - demonstrates how to use the command with proper syntax
    /// </summary>
    public string? Usage { get; set; }
    
    /// <summary>
    /// Gets or sets the command aliases - alternative names that can be used to invoke the same command
    /// </summary>
    public string[] Aliases { get; set; } = [];
    
    /// <summary>
    /// Gets or sets whether the command is hidden from help - controls visibility in help listings
    /// </summary>
    public bool IsHidden { get; set; }
    
    /// <summary>
    /// Gets or sets the command category for organization - groups related commands together in help
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Basic CommandAttribute constructor for defining a CLI command with name and optional description
    /// </summary>
    /// <param name="name">The primary name of the command</param>
    /// <param name="description">Optional description of what the command does</param>
    public CommandAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Attribute for defining command parameters - positional arguments that commands accept
/// Applied to properties to define how command-line arguments are mapped to command properties
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParameterAttribute : Attribute
{
    /// <summary>
    /// Gets the parameter name - used in help and documentation to identify the parameter
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the parameter description - explains the purpose and expected value of the parameter
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets whether the parameter is required - determines if the command fails when parameter is missing
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the parameter default value - value used when parameter is not provided
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Basic ParameterAttribute constructor for defining a command parameter with name and optional description
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="description">Optional description of the parameter's purpose</param>
    public ParameterAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Attribute for defining command options (flags) - named, optional arguments that modify command behavior
/// Applied to properties to define flags that can be passed to commands with --name or -shortName syntax
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OptionAttribute : Attribute
{
    /// <summary>
    /// Gets the option name - the long form name used with -- prefix
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the option description - explains what effect the option has when used
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets the short name alias - single character alias used with - prefix
    /// </summary>
    public char ShortName { get; set; }

    /// <summary>
    /// Basic OptionAttribute constructor for defining a command option with name and optional description
    /// </summary>
    /// <param name="name">The long name of the option</param>
    /// <param name="description">Optional description of what the option does</param>
    public OptionAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Represents comprehensive command metadata extracted from CommandAttribute and related attributes
/// Serves as a structured container for all information about a command used by the command system
/// </summary>
public class CommandMetadata
{
    /// <summary>
    /// Gets or sets the command name - primary identifier for the command
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the command description - explains the command's purpose and functionality
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the command usage examples - shows proper command syntax with examples
    /// </summary>
    public string? Usage { get; set; }
    
    /// <summary>
    /// Gets or sets the command aliases - alternative names that invoke the same command
    /// </summary>
    public string[] Aliases { get; set; } = [];
    
    /// <summary>
    /// Gets or sets whether the command is hidden from help - controls if command appears in help output
    /// </summary>
    public bool IsHidden { get; set; }
    
    /// <summary>
    /// Gets or sets the command category for organization - groups related commands in help system
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Gets or sets the command type - the actual Type that implements the command logic
    /// </summary>
    public Type CommandType { get; set; } = null!;
}