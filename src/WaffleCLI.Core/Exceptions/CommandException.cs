namespace WaffleCLI.Core.Exceptions;

/// <summary>
/// Represents errors that occur during command execution
/// </summary>
public class CommandException : Exception
{
    /// <summary>
    /// Gets the command name that caused the exception
    /// </summary>
    public string CommandName { get; }
    
    /// <summary>
    /// Gets the exit code for the command failure
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Initializes a new instance of the CommandException class
    /// </summary>
    /// <param name="commandName">Command name</param>
    /// <param name="message">Error message</param>
    /// <param name="exitCode">Exit code</param>
    public CommandException(string commandName, string message, int exitCode = 1) :  base(message)
    {
        CommandName = commandName;
        ExitCode = exitCode;
    }
    
    /// <summary>
    /// Initializes a new instance of the CommandException class
    /// </summary>
    /// <param name="commandName">Command name</param>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="exitCode">Exit code</param>
    public CommandException(string commandName, string message, Exception innerException, int exitCode = 1) 
        : base(message, innerException)
    {
        CommandName = commandName;
        ExitCode = exitCode;
    }
}