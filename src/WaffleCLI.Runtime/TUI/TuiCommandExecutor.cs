using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Core.Parsers;

namespace WaffleCLI.Runtime.TUI;

/// <summary>
/// Provides command execution functionality specifically designed for TUI environments.
/// </summary>
/// <remarks>
/// Extends the standard command execution with middleware pipeline support and enhanced error handling
/// suitable for interactive text user interface applications.
/// </remarks>
public class TuiCommandExecutor : ICommandExecutor
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger<TuiCommandExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<CommandContext, Task> _middlewarePipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="TuiCommandExecutor"/> class.
    /// </summary>
    /// <param name="commandRegistry">The command registry for retrieving command instances.</param>
    /// <param name="logger">The logger for recording execution events and errors.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="middlewares">The collection of command middlewares to apply during execution.</param>
    public TuiCommandExecutor(ICommandRegistry commandRegistry, ILogger<TuiCommandExecutor> logger,
        IServiceProvider serviceProvider, IEnumerable<ICommandMiddleware> middlewares)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _middlewarePipeline = BuildMiddlewarePipeline(middlewares);
    }

    /// <summary>
    /// Executes a command from a command line string.
    /// </summary>
    /// <param name="commandLine">The full command line string to execute.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>A task that represents the asynchronous execution operation, containing the command result.</returns>
    /// <remarks>
    /// Parses the command line into command name and arguments, then delegates to the command-specific execution method.
    /// Returns error results for empty or invalid command lines.
    /// </remarks>
    public async Task<CommandResult> ExecuteAsync(string commandLine, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) return CommandResult.ErrorResult("Empty command line");

        var parts = CommandLineParser.Parse(commandLine);
        if (parts.Length == 0) return CommandResult.ErrorResult("Invalid command format");
        
        var commandName = parts[0];
        var args = parts.Length > 1 ? parts[1..] : [];
        
        return await ExecuteAsync(commandName, args, token);
    }

    /// <summary>
    /// Executes a command with specified name and arguments.
    /// </summary>
    /// <param name="command">The name of the command to execute.</param>
    /// <param name="args">The arguments to pass to the command.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>A task that represents the asynchronous execution operation, containing the command result.</returns>
    /// <remarks>
    /// Creates a command context and processes it through the middleware pipeline.
    /// Handles exceptions gracefully and returns appropriate error results.
    /// </remarks>
    public async Task<CommandResult> ExecuteAsync(string command, string[] args, CancellationToken token = default)
    {
        var context = new CommandContext
        {
            CommandLine = $"{command} {string.Join(" ", args)}",
            CommandName = command,
            Arguments = args,
            CancellationToken = token,
            ServiceProvider = _serviceProvider
        };

        try
        {
            await _middlewarePipeline(context);

            return context.Result ?? CommandResult.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {Command}", command);
            return CommandResult.ErrorResult($"Execution error: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds the middleware pipeline for command execution.
    /// </summary>
    /// <param name="middlewares">The collection of middlewares to include in the pipeline.</param>
    /// <returns>A function that represents the complete middleware pipeline.</returns>
    /// <remarks>
    /// Constructs a pipeline that includes command resolution, command execution, and all registered middlewares.
    /// The pipeline processes command contexts in sequence, allowing each middleware to intercept and modify execution.
    /// </remarks>
    private Func<CommandContext, Task> BuildMiddlewarePipeline(IEnumerable<ICommandMiddleware> middlewares)
    {
        var pipeline = new MiddlewarePipeline();
        
        pipeline.Use(async (context, next) =>
        {
            var commandInstance = _commandRegistry.GetCommand(context.CommandName);
            if (commandInstance == null)
            {
                context.Result = CommandResult.ErrorResult($"Command not found: {context.CommandName}");
                return;
            }
            
            context.Command = commandInstance;
            await next();
        });
        
        pipeline.Use(async (context, next) =>
        {
            if (context.Command != null && !context.IsHandled)
            {
                await context.Command.ExecuteAsync(context.Arguments, context.CancellationToken);
            }
            await next();
        });
        
        foreach (var middleware in middlewares)
        {
            var currentMiddleware = middleware;
            pipeline.Use(async (context, next) =>
            {
                await currentMiddleware.InvokeAsync(context, next);
            });
        }
        
        return pipeline.Build();
    }
}