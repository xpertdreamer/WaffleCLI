namespace WaffleCLI.Abstractions.Commands;

/// <summary>
/// Represents a command that can be executed in the console framework
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the name of the command
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the description of the command
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Executes the command with the specified arguments
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task ExecuteAsync(string[] args, CancellationToken token = default);
}