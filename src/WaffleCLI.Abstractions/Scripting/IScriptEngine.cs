namespace WaffleCLI.Abstractions.Scripting;

/// <summary>
/// Executes script files containing multiple commands in sequence
/// Provides functionality to run, validate, and manage command scripts
/// </summary>
public interface IScriptEngine
{
    /// <summary>
    /// Executes commands from a script file - reads file content and processes each command line by line
    /// </summary>
    /// <param name="filePath">Path to the script file containing commands to execute</param>
    /// <param name="cancellationToken">Token to cancel script execution</param>
    /// <returns>Script execution result with detailed outcomes of all commands</returns>
    Task<ScriptResult> ExecuteScriptAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes commands from script content - processes provided command strings directly
    /// </summary>
    /// <param name="commands">Array of command strings to execute in sequence</param>
    /// <param name="token">Token to cancel script execution</param>
    /// <returns>Script execution result with detailed outcomes of all commands</returns>
    Task<ScriptResult> ExecuteScriptAsync(string[] commands, CancellationToken token = default);
    
    /// <summary>
    /// Validates a script file without executing - checks syntax, command existence, and potential issues
    /// </summary>
    /// <param name="filePath">Path to the script file to validate</param>
    /// <returns>Validation result indicating script validity and any errors or warnings</returns>
    ScriptValidationResult ValidateScript(string filePath);
}

/// <summary>
/// Represents the comprehensive result of script execution including success status, metrics, and individual command outcomes
/// Contains aggregated information about the entire script execution process
/// </summary>
public class ScriptResult
{
    /// <summary>
    /// Gets or sets whether the entire script execution was successful (all commands completed without critical errors)
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the number of commands that were successfully executed
    /// </summary>
    public int ExecutedCommands { get; set; }
    
    /// <summary>
    /// Gets or sets the number of commands that failed during execution
    /// </summary>
    public int FailedCommands { get; set; }
    
    /// <summary>
    /// Gets or sets the total time taken to execute the entire script
    /// </summary>
    public TimeSpan TotalDuration { get; set; }
    
    /// <summary>
    /// Gets or sets the detailed results for each individual command execution in the script
    /// </summary>
    public List<CommandExecutionResult> CommandExecutionResults { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the duration of script execution (alias for TotalDuration)
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Represents the result of a single command execution within a script
/// Provides detailed information about command outcome, output, errors, and performance
/// </summary>
public class CommandExecutionResult
{
    /// <summary>
    /// Gets or sets the command string that was executed
    /// </summary>
    public string Command { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether the command execution was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the standard output produced by the command execution
    /// </summary>
    public string? Output { get; set; }
    
    /// <summary>
    /// Gets or sets any error output produced by the command execution
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// Gets or sets the time taken to execute this specific command
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Represents the result of script validation including validity status, errors, and warnings
/// Used to verify script correctness before execution and identify potential issues
/// </summary>
public class ScriptValidationResult
{
    /// <summary>
    /// Gets or sets whether the script is valid and can be executed
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets the list of validation errors that prevent script execution
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of validation warnings that don't prevent execution but indicate potential issues
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Exception thrown when script execution fails due to command errors, validation issues, or execution problems
/// Provides detailed information about the failure location and context
/// </summary>
public class ScriptExecutionException : Exception
{
    /// <summary>
    /// Gets the command that caused the execution failure
    /// </summary>
    public string Command { get; }
    
    /// <summary>
    /// Gets the line number in the script where the failure occurred
    /// </summary>
    public int LineNumber { get; }
    
    /// <summary>
    /// Initializes a new instance of ScriptExecutionException with command context and error message
    /// </summary>
    /// <param name="command">The command that failed execution</param>
    /// <param name="lineNumber">The line number where the failure occurred</param>
    /// <param name="message">The error message describing the failure</param>
    public ScriptExecutionException(string command, int lineNumber, string message) 
        : base($"Script failed at line {lineNumber}: {message}")
    {
        Command = command;
        LineNumber = lineNumber;
    }
}