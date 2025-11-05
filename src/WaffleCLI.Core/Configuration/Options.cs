namespace WaffleCLI.Core.Configuration;

/// <summary>
/// Represents the root configuration options for the WaffleCLI application.
/// </summary>
/// <remarks>
/// Contains nested configuration sections for command registration, host behavior,
/// logging, and scripting capabilities.
/// </remarks>
public class CliOptions
{
    /// <summary>
    /// Gets or sets the options for command registration and discovery.
    /// </summary>
    public CommandRegistrationOptions CommandRegistration { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the options for host behavior and user interface.
    /// </summary>
    public HostOptions Host { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the options for logging configuration.
    /// </summary>
    public LoggingOptions Logging { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the options for scripting capabilities.
    /// </summary>
    public ScriptingOptions Scripting { get; set; } = new();
}

/// <summary>
/// Represents configuration options for command registration and discovery.
/// </summary>
/// <remarks>
/// Controls how commands are discovered, registered, and filtered within the application.
/// </remarks>
public class CommandRegistrationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically discover commands using reflection.
    /// </summary>
    public bool AutoDiscoverCommands { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the array of assembly names to scan for commands.
    /// </summary>
    public string[] AssembliesToScan { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the array of command names to exclude from registration.
    /// </summary>
    public string[] ExcludedCommands { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets a value indicating whether to use naming conventions for command discovery.
    /// </summary>
    public bool UseNamingConvention { get; set; } = true;
}

/// <summary>
/// Represents configuration options for host behavior and user interface.
/// </summary>
/// <remarks>
/// Controls the interactive shell experience, including welcome messages and prompt configuration.
/// </remarks>
public class HostOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to show the welcome message on application start.
    /// </summary>
    public bool ShowWelcomeMessage { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the custom welcome message to display.
    /// </summary>
    public string WelcomeMessage { get; set; } = "WaffleCLI";
    
    /// <summary>
    /// Gets or sets the command prompt string.
    /// </summary>
    public string Prompt { get; set; } = "> ";
    
    /// <summary>
    /// Gets or sets a value indicating whether to exit the application on non-zero command exit codes.
    /// </summary>
    public bool ExitOnNonZeroExitCode { get; set; } = false;
}

/// <summary>
/// Represents configuration options for logging behavior.
/// </summary>
/// <remarks>
/// Controls logging levels, verbosity, and specific logging features for command execution.
/// </remarks>
public class LoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled.
    /// </summary>
    public bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the minimum log level for output.
    /// </summary>
    public string MinimumLevel { get; set; } = "Warning";
    
    /// <summary>
    /// Gets or sets a value indicating whether to log command execution details.
    /// </summary>
    public bool LogCommandExecution { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to log performance metrics.
    /// </summary>
    public bool LogPerformance { get; set; } = false;
}

/// <summary>
/// Represents configuration options for scripting capabilities.
/// </summary>
/// <remarks>
/// Controls script execution behavior, file extensions, and execution limits.
/// </remarks>
public class ScriptingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether scripting capabilities are enabled.
    /// </summary>
    public bool EnableScripting { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the default file extension for script files.
    /// </summary>
    public string DefaultScriptExtension { get; set; } = ".waffle";
    
    /// <summary>
    /// Gets or sets a value indicating whether to stop script execution on the first command failure.
    /// </summary>
    public bool StopOnFirstFailure { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum execution time in seconds for scripts.
    /// </summary>
    public int MaxExecutionTimeSeconds { get; set; } = 300;
}