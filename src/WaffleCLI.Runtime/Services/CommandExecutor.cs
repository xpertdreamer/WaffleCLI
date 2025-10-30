using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaffleCLI.Runtime.Options;
using WaffleCLI.Runtime.Parsers;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Executes commands by resolving them from the command registry and handling their execution lifecycle.
/// Provides error handling, logging, and result processing for command operations.
/// </summary>
public class CommandExecutor : ICommandExecutor
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger<CommandExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandExecutor"/> class.
    /// </summary>
    /// <param name="commandRegistry">The registry used to resolve command instances.</param>
    /// <param name="logger">The logger for recording command execution events.</param>
    /// <param name="options">Configuration options for command execution behavior.</param>
    public CommandExecutor(ICommandRegistry commandRegistry, ILogger<CommandExecutor> logger,
        IOptions<CommandExecutorOptions> options)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Executes a command by parsing the provided command line string into command name and arguments.
    /// </summary>
    /// <param name="commandLine">The full command line string to execute.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>
    /// A <see cref="CommandResult"/> indicating the outcome of the command execution.
    /// Returns an error result if the command line is empty or invalid.
    /// </returns>
    public async Task<CommandResult> ExecuteAsync(string commandLine, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return CommandResult.ErrorResult("Empty command line");

        var parts = CommandLineParser.Parse(commandLine);
        if (parts.Length == 0)
            return CommandResult.ErrorResult("Invalid command format");
        
        var commandName = parts[0];
        var args = parts.Length > 1 ? parts[1..] : [];
        
        return await ExecuteAsync(commandName, args, token);
    }

    /// <summary>
    /// Executes a command with the specified name and arguments.
    /// </summary>
    /// <param name="command">The name of the command to execute.</param>
    /// <param name="args">The arguments to pass to the command.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>
    /// A <see cref="CommandResult"/> indicating the outcome of the command execution.
    /// Handles various exception types and returns appropriate error results with logging.
    /// </returns>
    /// <remarks>
    /// <para>This method handles the following scenarios:</para>
    /// <list type="bullet">
    /// <item><description>Command not found in registry</description></item>
    /// <item><description><see cref="CommandException"/> with specific error messages and exit codes</description></item>
    /// <item><description>Unexpected exceptions during command execution</description></item>
    /// </list>
    /// </remarks>
    public async Task<CommandResult> ExecuteAsync(string command, string[] args, CancellationToken token = default)
    {
        try
        {
            var commandInstance = _commandRegistry.GetCommand(command);
            if (commandInstance == null)
                return CommandResult.ErrorResult($"Command not found: {command}");

            _logger.LogDebug("Execute command: {Command} with args: {Args} ", command, args);
            await commandInstance.ExecuteAsync(args, token);
            return CommandResult.SuccessResult();
        }
        catch (CommandException ex)
        {
            _logger.LogError(ex, "Command execution failed: {Command}", command);
            return CommandResult.ErrorResult(ex.Message, ex.ExitCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {Command}", command);
            return CommandResult.ErrorResult($"Execution error: {ex.Message}");
        }
    }
}