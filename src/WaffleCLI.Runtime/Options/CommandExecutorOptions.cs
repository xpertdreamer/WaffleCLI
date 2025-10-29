namespace WaffleCLI.Runtime.Options;

/// <summary>
/// Options for configuring command execution
/// </summary>
public class CommandExecutorOptions
{
    /// <summary>
    /// Gets or sets the default command timeout
    /// </summary>
    public TimeSpan DefaultTimeout {get;set;} = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Gets or sets whether to allow parallel command execution
    /// </summary>
    public bool AllowParallelExecution {get;set;} = false;
}