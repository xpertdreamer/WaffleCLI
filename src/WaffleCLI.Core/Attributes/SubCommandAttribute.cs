namespace WaffleCLI.Core.Attributes;

/// <summary>
/// Specifies that command is a subcommand of a command group
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SubCommandAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the parent command group
    /// </summary>
    public string ParentGroup { get; }
    
    /// <summary>
    /// Gets the name of the subcommand (optional, uses class name by default)
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Initializes a new instance of the SubCommandAttribute class
    /// </summary>
    /// <param name="parentGroup">The name of the parent command group</param>
    /// <param name="name">The name of the subcommand (optional)</param>
    public SubCommandAttribute(string parentGroup, string? name = null)
    {
        ParentGroup = parentGroup;
        Name = name;
    }
}