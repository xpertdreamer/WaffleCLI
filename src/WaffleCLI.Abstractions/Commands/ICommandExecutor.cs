namespace WaffleCLI.Abstractions.Commands;

/// <summary>
/// Executes commands and handles their lifecycle
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executes a command by parsing the provided command line string
    /// </summary>
    /// <param name="commandLine">The full command line string to execute</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Command execution result</returns>
    Task<CommandResult> ExecuteAsync(string commandLine, CancellationToken token = default);
    
    /// <summary>
    /// Executes a command with the specified name and arguments
    /// </summary>
    /// <param name="command">The name of the command to execute</param>
    /// <param name="args">The arguments to pass to the command</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Command execution result</returns>
    Task<CommandResult> ExecuteAsync(string command, string[] args, CancellationToken token = default);
}

/// <summary>
/// Represents the result of a command execution
/// </summary>
public class CommandResult
{
    /// <summary>
    /// Gets a value indicating whether the command executed successfully
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Gets the message associated with the command execution
    /// </summary>
    public string? Message { get; }
    
    /// <summary>
    /// Gets the exit code of the command execution
    /// </summary>
    public int ExitCode { get; }

    private CommandResult(bool success, string? message, int exitCode)
    {
        Success = success;
        Message = message;
        ExitCode = exitCode;
    }

    /// <summary>
    /// Creates a successful command result
    /// </summary>
    /// <param name="message">Optional success message</param>
    /// <returns>Command result indicating success</returns>
    public static CommandResult SuccessResult(string? message = null)
    {
        return new CommandResult(true, message, 0);
    }

    /// <summary>
    /// Creates an error command result
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="exitCode">Exit code (default is 1)</param>
    /// <returns>Command result indicating error</returns>
    public static CommandResult ErrorResult(string message, int exitCode = 1)
    {
        return new CommandResult(false, message, exitCode);
    }
}