namespace WaffleCLI.Core.Middleware;

/// <summary>
/// Represents middleware that can intercept and process command execution
/// </summary>
public interface ICommandMiddleware
{
    /// <summary>
    /// Executes the middleware
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <param name="next">Next middleware in pipeline</param>
    Task InvokeAsync(CommandContext context, Func<Task> next); 
}