namespace WaffleCLI.Core.Models;

/// <summary>
/// Enchanced command attribute with additioanl metadata
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the command description
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets the command usage examples
    /// </summary>
    public string? Usage { get; set; }
    
    /// <summary>
    /// Gets or sets the command aliases
    /// </summary>
    public string[] Aliases { get; set; } = [];
    
    /// <summary>
    /// Gets or sets whether the command is hidden from help
    /// </summary>
    public bool IsHidden { get; set; }
    
    /// <summary>
    /// Gets or sets the command category for organization
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Basic CmdAttribute constructor 
    /// </summary>
    public CommandAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Attribute for command parameters
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParameterAttribute : Attribute
{
    /// <summary>
    /// Gets the parameter name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the parameter description
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets whether the parameter is required
    /// </summary>
    public bool IsRequired { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the parameter default value
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Basic ParameterAttribute constructor
    /// </summary>
    public ParameterAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Attribute for command options (flags)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OptionAttribute : Attribute
{
    /// <summary>
    /// Gets the option name
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the option description
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets the short name alias
    /// </summary>
    public char ShortName { get; set; }

    public OptionAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}


/// <summary>
/// Represents command metadata
/// </summary>
public class CommandMetadata
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Usage { get; set; }
    public string[] Aliases { get; set; } = [];
    public bool IsHidden { get; set; }
    public string? Category { get; set; }
    public Type CommandType { get; set; } = null!;
}