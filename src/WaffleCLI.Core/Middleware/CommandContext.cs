using WaffleCLI.Abstractions.Commands;

namespace WaffleCLI.Core.Middleware;

/// <summary>
/// Unified command execution context for middleware pipeline
/// </summary>
public class CommandContext
{
    /// <summary>
    /// Gets or sets the command line
    /// </summary>
    public string CommandLine { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the command name
    /// </summary>
    public string CommandName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the command arguments
    /// </summary>
    public string[] Arguments { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the command instance
    /// </summary>
    public ICommand? Command { get; set; }
    
    /// <summary>
    /// Gets or sets the cancellation token
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
    
    /// <summary>
    /// Gets or sets the service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets whether the command execution was handled
    /// </summary>
    public bool IsHandled { get; set; }
    
    /// <summary>
    /// Gets or sets the execution result
    /// </summary>
    public CommandResult? Result { get; set; }
    
    /// <summary>
    /// Additional properties for middleware communication
    /// </summary>
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
}