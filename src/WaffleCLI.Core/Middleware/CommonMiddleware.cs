using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Exceptions;
using WaffleCLI.Core.Output;

namespace WaffleCLI.Core.Middleware;

/// <summary>
/// Middleware for logging command execution
/// </summary>
public class LoggingMiddleware : ICommandMiddleware
{
    private readonly IConsoleOutput _output;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(IConsoleOutput output, ILogger<LoggingMiddleware> logger)
    {
        _output = output;
        _logger = logger;
    }
    
    /// <summary>
    /// Executes the middleware
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="next">Next middleware in pipeline</param>
    public async Task InvokeAsync(CommandContext context, Func<Task> next)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Executing command: {CommandName} with args: {Arguments}", 
            context.CommandName, context.Arguments);
        
        try
        {
            await next();
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Command {CommandName} completed in {Duration}ms", 
                context.CommandName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Command {CommandName} failed after {Duration}ms", 
                context.CommandName, duration.TotalMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Middleware for handling exceptions
/// </summary>
public class ExceptionHandlingMiddleware : ICommandMiddleware
{
    private readonly IConsoleOutput _output;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="output"></param>
    public ExceptionHandlingMiddleware(IConsoleOutput output)
    {
        _output = output;
    }
    
    /// <summary>
    /// Executes the middleware
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="next">Next middleware in pipeline</param>
    public async Task InvokeAsync(CommandContext context, Func<Task> next)
    {
        try
        {
            await next();
        }
        catch (CommandException ex)
        {
            _output.WriteError($"Command error: {ex.Message}");
            context.Result = CommandResult.ErrorResult(ex.Message, ex.ExitCode);
        }
        catch (Exception ex)
        {
            _output.WriteError($"Unexpected error: {ex.Message}");
            context.Result = CommandResult.ErrorResult($"Execution failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Middleware for timing command execution
/// </summary>
public class TimingMiddleware : ICommandMiddleware
{
    private readonly IConsoleOutput _output;

    public TimingMiddleware(IConsoleOutput output)
    {
        _output = output;
    }
    
    /// <summary>
    /// Executes the middleware
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="next">Next middleware in pipeline</param>
    public async Task InvokeAsync(CommandContext context, Func<Task> next)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        await next();
        
        stopwatch.Stop();
        context.Properties["ExecutionTime"] = stopwatch.Elapsed;
        _output.WriteInfo($"Command completed in {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
    }
}

/// <summary>
/// Middleware for input validation
/// </summary>
public class ValidationMiddleware : ICommandMiddleware
{
    private readonly IConsoleOutput _output;

    public ValidationMiddleware(IConsoleOutput output)
    {
        _output = output;
    }
    
    /// <summary>
    /// Executes the middleware
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="next">Next middleware in pipeline</param>
    public async Task InvokeAsync(CommandContext context, Func<Task> next)
    {
        if (string.IsNullOrWhiteSpace(context.CommandName))
        {
            context.Result = CommandResult.ErrorResult("Command name cannot be empty");
            return;
        }

        await next();
    }
}