namespace WaffleCLI.Core.Attributes;

/// <summary>
/// Provides attributes for defining commands, command groups, and subcommands in a CLI application.
/// </summary>

/// <summary>
/// Indicates that a class represents a CLI command.
/// </summary>
/// <remarks>
/// This attribute is applied to classes that implement command execution logic.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets the unique command name for invocation from CLI.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the brief description of the command's purpose displayed in help systems.
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets or sets the array of alternative names for invoking the command.
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets a value indicating whether the command should be hidden from help systems.
    /// </summary>
    public bool IsHidden { get; set; }
    
    /// <summary>
    /// Gets or sets the category for grouping commands in help systems.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The unique name of the command. Cannot be null or empty.</param>
    /// <param name="description">Optional description of the command displayed in help systems.</param>
    public CommandAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Indicates that a class represents a command group that can contain subcommands.
/// Inherits all properties from the base command.
/// </summary>
/// <remarks>
/// Command groups are used to organize related commands in a hierarchical structure.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class CommandGroupAttribute : CommandAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandGroupAttribute"/> class.
    /// </summary>
    /// <param name="name">The unique name of the command group. Cannot be null or empty.</param>
    /// <param name="description">Optional description of the command group displayed in help systems.</param>
    public CommandGroupAttribute(string name, string? description = null) 
        : base(name, description) { }
}

/// <summary>
/// Indicates that a class represents a subcommand belonging to a specific command group.
/// </summary>
/// <remarks>
/// Subcommands can only be invoked within the context of their parent group.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class SubCommandAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the parent command group to which this subcommand belongs.
    /// </summary>
    public string ParentGroup { get; }
    
    /// <summary>
    /// Gets the local name of the subcommand within the parent group. If null, the class name is used.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubCommandAttribute"/> class.
    /// </summary>
    /// <param name="parentGroup">The name of the parent command group to which the subcommand belongs.</param>
    /// <param name="name">Optional name of the subcommand. If not specified, the class name is used.</param>
    public SubCommandAttribute(string parentGroup, string? name = null)
    {
        ParentGroup = parentGroup;
        Name = name;
    }
}