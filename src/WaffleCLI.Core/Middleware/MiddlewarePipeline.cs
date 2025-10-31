namespace WaffleCLI.Core.Middleware;

/// <summary>
/// Builds the middleware pipeline
/// </summary>
public class MiddlewarePipeline
{
    // Stores the middleware components as a list of functions that wrap the next middleware in the chain
    private readonly List<Func<Func<CommandContext, Task>, Func<CommandContext, Task>>> _components = new();

    /// <summary>
    /// Adds a middleware to the pipeline
    /// </summary>
    public MiddlewarePipeline Use(Func<CommandContext, Func<Task>, Task> middleware)
    {
        // Wrap the middleware function to integrate it into the pipeline chain
        _components.Add(next => 
        {
            // Return a new function that takes a CommandContext and executes the middleware
            return context => middleware(context, () => next(context));
        });
        return this;
    }

    /// <summary>
    /// Builds the pipeline
    /// </summary>
    public Func<CommandContext, Task> Build()
    {
        // Start with a terminal middleware that does nothing (end of pipeline)
        Func<CommandContext, Task> pipeline = context => Task.CompletedTask;
        
        // Build pipeline in reverse order (first middleware becomes the outer layer)
        // This ensures that the first middleware added is the first to execute when the pipeline runs
        for (int i = _components.Count - 1; i >= 0; i--)
        {
            // Wrap the current pipeline with the next middleware component
            pipeline = _components[i](pipeline);
        }
        
        return pipeline;
    }
}