namespace WaffleCLI.Core.Models;

/// <summary>
/// Represents the context for command execution
/// </summary>
public class CommandContext
{
    /// <summary>
    /// Gets or sets the command line
    /// </summary>
    public string CommandLine { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the command arguments
    /// </summary>
    public string[] Arguments {get;set;} = Array.Empty<string>();
    
        
    /// <summary>
    /// Gets or sets the cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    
    /// <summary>
    /// Gets or sets the service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; } = null!;
}