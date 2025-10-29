namespace WaffleCLI.Abstractions.Hosting;

/// <summary>
/// Represents the main console application host
/// </summary>
public interface IConsoleHost
{
    /// <summary>
    /// Runs the console application
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>Exit code</returns>
    Task<int> RunAsync(CancellationToken token = default);
    
    /// <summary>
    /// Executes a single command
    /// </summary>
    /// <param name="commandLine">Command line to execute</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Exit code</returns>
    Task<int> ExecuteCommandAsync(string commandLine, CancellationToken token = default);
}