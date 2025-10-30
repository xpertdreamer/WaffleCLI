namespace WaffleCLI.Core.Middleware;

/// <summary>
/// Builds the middleware pipeline
/// </summary>
public class MiddlewarePipeline
{
    private readonly List<Func<Func<CommandContext, Task>, Func<CommandContext, Task>>> _components = [];
    
    /// <summary>
    /// Adds a middleware to the pipeline
    /// </summary>
    public MiddlewarePipeline Use(Func<CommandContext, Func<Task>, Task> middleware)
    {
        _components.Add(next => context => middleware(context, () => next(context)));
        return this;
    }
    
    /// <summary>
    /// Builds the pipeline
    /// </summary>
    public Func<CommandContext, Task> Build()
    {
        Func<CommandContext, Task> pipeline = context => Task.CompletedTask;
        
        for (int i = _components.Count - 1; i >= 0; i--)
        {
            pipeline = _components[i](pipeline);
        }
        
        return pipeline;
    }
}