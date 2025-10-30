namespace WaffleCLI.Core.Attributes;

/// <summary>
/// Specifies the metadata for a command including name and description
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
    /// Initializes a new instance of the CommandAttribute class
    /// </summary>
    /// <param name="name">Command name</param>
    /// <param name="description">Command description</param>
    public CommandAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
}