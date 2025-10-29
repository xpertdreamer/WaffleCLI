using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using WaffleCLI.Runtime.Options;

namespace WaffleCLI.Runtime.Services;

internal class DefaultConsoleHost : IConsoleHost
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger<DefaultConsoleHost> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ICommandRegistry _commandRegistry;
    private readonly IOptions<ConsoleHostOptions> _options;
    
    public DefaultConsoleHost(
        ICommandExecutor commandExecutor,
        ILogger<DefaultConsoleHost> logger,
        IHostApplicationLifetime lifetime,
        ICommandRegistry commandRegistry,
        IOptions<ConsoleHostOptions> options)
    {
        _commandExecutor = commandExecutor;
        _logger = logger;
        _lifetime = lifetime;
        _commandRegistry = commandRegistry;
        _options = options;
    }

    public async Task<int> RunAsync(CancellationToken token = default)
    {
        try
        {
            if (_options.Value.ShowWelcomeMessage)
            {
                AnsiConsole.Write(new FigletText("WaffleCLI").Color(Color.Blue));
                AnsiConsole.MarkupLine("[green]WaffleCLI started. Type 'help' to see available commands.[/]");
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
                        AnsiConsole.MarkupLine($"[red]Error: {result.Message}[/]");
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
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                }
            }

            return 0;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    public async Task<int> ExecuteCommandAsync(string commandLine, CancellationToken token = default)
    {
        var result = await _commandExecutor.ExecuteAsync(commandLine, token);
        return result.ExitCode;
    }
}