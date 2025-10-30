using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Options;

namespace WaffleCLI.Runtime.Services;

/// <summary>
/// 
/// </summary>
public class DefaultConsoleHost : IConsoleHost
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger<DefaultConsoleHost> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IConsoleOutput _output;
    private readonly IOptions<ConsoleHostOptions> _options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandExecutor"></param>
    /// <param name="logger"></param>
    /// <param name="lifetime"></param>
    /// <param name="options"></param>
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
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
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

                    var result = await _commandExecutor.ExecuteAsync(input, token);

                    if (!result.Success && !string.IsNullOrEmpty(result.Message) ||
                        !string.IsNullOrEmpty(result.Message))
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
    /// 
    /// </summary>
    /// <param name="commandLine"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<int> ExecuteCommandAsync(string commandLine, CancellationToken token = default)
    {
        var result = await _commandExecutor.ExecuteAsync(commandLine, token);
        return result.ExitCode;
    }

    private void ShowWelcomeMessage()
    {
        _output.WriteLine("=========================================", ConsoleColor.Blue);
        _output.WriteLine("        WaffleCLI", ConsoleColor.Blue);
        _output.WriteLine("=========================================", ConsoleColor.Blue);
        _output.WriteLine("WaffleCLI started. Type 'help' for available commands.", ConsoleColor.Green);
        _output.WriteLine();
    }
}