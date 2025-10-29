using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaffleCLI.Runtime.Options;
using WaffleCLI.Runtime.Parsers;

namespace WaffleCLI.Runtime.Services;

internal class CommandExecutor : ICommandExecutor
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger<CommandExecutor> _logger;
    private readonly IOptions<CommandExecutorOptions> _options;

    public CommandExecutor(ICommandRegistry commandRegistry, ILogger<CommandExecutor> logger,
        IOptions<CommandExecutorOptions> options)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
        _options = options;
    }

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