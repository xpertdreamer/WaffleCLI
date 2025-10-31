namespace WaffleCLI.Abstractions.Scripting;

/// <summary>
/// Executes script files containing multiple commands
/// </summary>
public interface IScriptEngine
{
    /// <summary>
    /// Executes commands from a script files
    /// </summary>
    Task<ScriptResult> ExecuteScriptAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes commands from script content
    /// </summary>
    Task<ScriptResult> ExecuteScriptAsync(string[] commands, CancellationToken token = default);
    
    /// <summary>
    /// Validates a script file without executing
    /// </summary>
    ScriptValidationResult ValidateScript(string filePath);
}

/// <summary>
/// Represents the result of script execution
/// </summary>
public class ScriptResult
{
    public bool Success { get; set; }
    public int ExecutedCommands {get; set;}
    public int FailedCommands {get; set;}
    public TimeSpan TotalDeration {get; set;}
    public List<CommandExecutionResult> CommandExecutionResults { get; set; }
    public TimeSpan Duration {get; set;}
}

/// <summary>
/// Represents the result of a single command execution in a script
/// </summary>
public class CommandExecutionResult
{
    public string Command { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Represents script validation result
/// </summary>
public class ScriptValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Exception thrown when script execution fails
/// </summary>
public class ScriptExecutionException : Exception
{
    public string Command { get; }
    public int LineNumber { get; }
    
    public ScriptExecutionException(string command, int lineNumber, string message) 
        : base($"Script failed at line {lineNumber}: {message}")
    {
        Command = command;
        LineNumber = lineNumber;
    }
}

