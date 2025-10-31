using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Runtime.Options;
using WaffleCLI.Runtime.Parsers;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Enhanced command executor with middleware support
/// </summary>
public class CommandExecutor : ICommandExecutor
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger<CommandExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<CommandContext, Task> _middlewarePipeline; 
    
    /// <summary>
    /// Basic constructor
    /// </summary>
    public CommandExecutor(
        ICommandRegistry commandRegistry,
        ILogger<CommandExecutor> logger,
        IOptions<CommandExecutorOptions> options,
        IServiceProvider serviceProvider,
        IEnumerable<ICommandMiddleware> middlewares)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _middlewarePipeline = BuildMiddlewarePipeline(middlewares);
    }

    /// <summary>
    /// Executes command from command line string
    /// </summary>
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
    /// Executes command with specified name and arguments
    /// </summary>
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
            // Execute the middleware pipeline
            await _middlewarePipeline(context);
            
            // Return result if middleware set it
            if (context.Result != null)
            {
                return context.Result;
            }
            
            // Return success result if no result was set
            return CommandResult.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {Command}", command);
            return CommandResult.ErrorResult($"Execution error: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds the middleware pipeline
    /// </summary>
    private Func<CommandContext, Task> BuildMiddlewarePipeline(IEnumerable<ICommandMiddleware> middlewares)
    {
        var pipeline = new MiddlewarePipeline();
        
        // Add built-in command resolution middleware
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
        
        // Add actual command execution middleware
        pipeline.Use(async (context, next) =>
        {
            if (context.Command != null && !context.IsHandled)
            {
                await context.Command.ExecuteAsync(context.Arguments, context.CancellationToken);
            }
            await next();
        });
        
        // Add custom middlewares in the order they were registered
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