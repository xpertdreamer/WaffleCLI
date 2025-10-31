using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Options;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Default implementation of <see cref="IConsoleHost"/> that provides an interactive command-line interface
/// with command execution, input processing, and user interaction capabilities.
/// </summary>
public class DefaultConsoleHost : IConsoleHost
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger<DefaultConsoleHost> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IConsoleOutput _output;
    private readonly IOptions<ConsoleHostOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultConsoleHost"/> class.
    /// </summary>
    /// <param name="commandExecutor">The service responsible for executing commands.</param>
    /// <param name="logger">The logger for recording host lifecycle events.</param>
    /// <param name="lifetime">The application lifetime manager for controlling application shutdown.</param>
    /// <param name="output">The console output service for displaying messages to the user.</param>
    /// <param name="options">Configuration options for console host behavior.</param>
    public DefaultConsoleHost(
        ICommandExecutor commandExecutor,
        ILogger<DefaultConsoleHost> logger,
        IHostApplicationLifetime lifetime,
        IConsoleOutput output,
        IOptions<ConsoleHostOptions> options)
    {
        _commandExecutor = commandExecutor;
        _logger = logger;
        _lifetime = lifetime;
        _output = output;
        _options = options;
    }

    /// <summary>
    /// Runs the interactive console host, displaying a welcome message and processing user input in a loop
    /// until the user types 'exit' or the cancellation token is triggered.
    /// </summary>
    /// <param name="token">Cancellation token to gracefully stop the host.</param>
    /// <returns>
    /// An exit code indicating the application termination status.
    /// Returns 0 for normal shutdown, or the command exit code if configured to exit on non-zero results.
    /// </returns>
    /// <remarks>
    /// <para>The host provides the following features:</para>
    /// <list type="bullet">
    /// <item><description>Interactive command prompt</description></item>
    /// <item><description>Welcome message display (configurable)</description></item>
    /// <item><description>Command execution with result handling</description></item>
    /// <item><description>Error message display for failed commands</description></item>
    /// <item><description>Graceful shutdown on 'exit' command or cancellation</description></item>
    /// <item><description>Automatic application termination on non-zero exit codes (configurable)</description></item>
    /// </list>
    /// </remarks>
    public async Task<int> RunAsync(CancellationToken token = default)
    {
        try
        {
            if (_options.Value.ShowWelcomeMessage)
            {
                ShowWelcomeMessage();
            }

            while (!token.IsCancellationRequested)
            {
                try
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        break;

                    ClearPromptLine();
                    
                    var result = await _commandExecutor.ExecuteAsync(input, token);

                    if (!result.Success && !string.IsNullOrEmpty(result.Message))
                    {
                        _output.WriteError($"Error: {result.Message}");
                    }
                    else if (!string.IsNullOrEmpty(result.Message))
                    {
                        _output.WriteSuccess(result.Message);
                    }
                    
                    if (result.ExitCode != 0 && _options.Value.ExitOnNonZeroExitCode)
                    {
                        return result.ExitCode;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while executing command.");
                    _output.WriteError($"Unhandled exception: {ex.Message}");
                }
            }

            return 0;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    /// <summary>
    /// Executes a single command line and returns the exit code without starting the interactive loop.
    /// </summary>
    /// <param name="commandLine">The command line to execute.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>The exit code from the command execution.</returns>
    /// <remarks>
    /// This method is useful for non-interactive scenarios where a single command needs to be executed.
    /// </remarks>
    public async Task<int> ExecuteCommandAsync(string commandLine, CancellationToken token = default)
    {
        var result = await _commandExecutor.ExecuteAsync(commandLine, token);
        return result.ExitCode;
    }

    private void ClearPromptLine()
    {
        try
        {
            int currentLine = Console.CursorTop;
            
            Console.SetCursorPosition(0, currentLine);
            
            Console.Write(new string(' ', Console.WindowWidth - 1));
            
            Console.SetCursorPosition(0, currentLine);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to clear prompt line");
        }
    }
    
    /// <summary>
    /// Displays the welcome message with application branding and usage instructions.
    /// </summary>
    private void ShowWelcomeMessage()
    {
        _output.WriteLine("=========================================", ConsoleColor.Blue);
        _output.WriteLine("        WaffleCLI", ConsoleColor.Blue);
        _output.WriteLine("=========================================", ConsoleColor.Blue);
        _output.WriteLine("WaffleCLI started. Type 'help' for available commands.", ConsoleColor.Green);
        _output.WriteLine();
    }
}