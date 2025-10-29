namespace WaffleCLI.Abstractions.Commands;

/// <summary>
/// Executes commands and handles results
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executes a command line
    /// </summary>
    /// <param name="commandLine">Command line to execute</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Command execution result</returns>
    Task<CommandResult> ExecuteAsync(string commandLine, CancellationToken token = default);
    
    /// <summary>
    /// Executes a specific command with arguments
    /// </summary>
    /// <param name="command">Command name</param>
    /// <param name="args">Command arguments</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Command execution result</returns>
    Task<CommandResult> ExecuteAsync(string command, string[] args, CancellationToken token = default);
}

/// <summary>
/// Represents the result of command execution
/// </summary>
/// <param name="Success">Whether the command executed successfully</param>
/// <param name="Message">Optional message</param>
/// <param name="ExitCode">Exit code</param>
public record CommandResult(bool Success, string? Message = null, int ExitCode = 1)
{
    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static CommandResult SuccessResult(string? message = null, int exitCode = 0)
        => new(true, message, 0);
    
    /// <summary>
    /// Creates an error resut
    /// </summary>
    public static CommandResult ErrorResult(string? message = null, int exitCode = 1) =>
        new(false, message, exitCode);
}