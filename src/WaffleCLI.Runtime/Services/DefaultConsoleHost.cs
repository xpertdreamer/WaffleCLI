using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Options;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// Default implementation of the console host that provides an interactive command-line interface.
/// </summary>
/// <remarks>
/// This host supports both interactive mode with a prompt and direct command execution.
/// It handles command execution, error reporting, and provides a user-friendly interface
/// with welcome messages and proper console management.
/// </remarks>
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
    /// <param name="logger">The logger for recording host operations and errors.</param>
    /// <param name="lifetime">The application lifetime manager for controlling shutdown.</param>
    /// <param name="output">The console output service for writing messages.</param>
    /// <param name="options">The configuration options for the console host.</param>
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
    /// Runs the console host in interactive mode, displaying a prompt and processing user input.
    /// </summary>
    /// <param name="token">Cancellation token to stop the interactive session.</param>
    /// <returns>An exit code indicating the final status of the application.</returns>
    /// <remarks>
    /// Displays a welcome message, processes commands in a loop until cancellation or exit command,
    /// and handles both expected command errors and unexpected exceptions gracefully.
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
    /// Executes a single command line and returns the exit code.
    /// </summary>
    /// <param name="commandLine">The command line string to execute.</param>
    /// <param name="token">Cancellation token to cancel the command execution.</param>
    /// <returns>The exit code from the command execution.</returns>
    /// <remarks>
    /// This method is intended for non-interactive command execution, such as when running
    /// commands from scripts or other automation scenarios.
    /// </remarks>
    public async Task<int> ExecuteCommandAsync(string commandLine, CancellationToken token = default)
    {
        var result = await _commandExecutor.ExecuteAsync(commandLine, token);
        return result.ExitCode;
    }

    /// <summary>
    /// Clears the current prompt line from the console to prepare for command output.
    /// </summary>
    /// <remarks>
    /// This method handles console cursor positioning to overwrite the prompt line with spaces,
    /// ensuring clean output display. Catches and logs any console-related exceptions.
    /// </remarks>
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
    /// Displays the welcome message and basic usage instructions.
    /// </summary>
    /// <remarks>
    /// The welcome message is only displayed if configured in <see cref="ConsoleHostOptions.ShowWelcomeMessage"/>.
    /// Uses colored output to enhance readability and provide a welcoming user experience.
    /// </remarks>
    private void ShowWelcomeMessage()
    {
        _output.WriteLine("=========================================", ConsoleColor.Blue);
        _output.WriteLine("        WaffleCLI", ConsoleColor.Blue);
        _output.WriteLine("=========================================", ConsoleColor.Blue);
        _output.WriteLine("Type 'help' for available commands or 'exit' to quit.", ConsoleColor.Green);
        _output.WriteLine();
    }
}