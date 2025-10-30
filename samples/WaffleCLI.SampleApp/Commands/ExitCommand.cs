using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for gracefully exiting the application
/// </summary>
[Command("exit", "Exit the application")]
public class ExitCommand : ICommand
{
    private readonly IConsoleHost _host;
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the ExitCommand class
    /// </summary>
    /// <param name="host">Console host service</param>
    /// <param name="output">Console output service</param>
    public ExitCommand(IConsoleHost host, IConsoleOutput output)
    {
        _host = host;
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "exit";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Exit the application";

    /// <summary>
    /// Executes the exit command with farewell message
    /// </summary>
    /// <param name="args">Command arguments (not used)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        _output.WriteInfo("Goodbye! Closing application...");
        return Task.CompletedTask;
    }
}