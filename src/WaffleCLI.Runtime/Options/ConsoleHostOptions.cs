namespace WaffleCLI.Runtime.Options;

/// <summary>
/// Options for configuring the console host
/// </summary>
public class ConsoleHostOptions
{
    /// <summary>
    /// Gets or sets whether to show the welcome message
    /// </summary>
    public bool ShowWelcomeMessage { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to exit on non-zero exit code
    /// </summary>
    public bool ExitOnNonZeroExitCode {get; set; } = false;
}