using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using WaffleCLI.SampleApp.Models;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for performing database operations and displaying database configuration
/// </summary>
[Command("db", "Database operations")]
public class DatabaseCommand : ICommand
{
    private readonly DatabaseSettings _settings;
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the DatabaseCommand class
    /// </summary>
    /// <param name="settings">Database settings</param>
    /// <param name="output">Console output service</param>
    public DatabaseCommand(DatabaseSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "db";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "Database operations";

    /// <summary>
    /// Executes database operations based on provided subcommands
    /// </summary>
    /// <param name="args">Subcommands: connect, settings, or empty for info</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length == 0)
        {
            ShowDatabaseInfo();
            return;
        }

        switch (args[0].ToLower())
        {
            case "connect":
                await TestConnectionAsync(cancellationToken);
                break;
            case "settings":
                ShowSettings();
                break;
            default:
                _output.WriteError("Unknown subcommand. Use: connect, settings");
                break;
        }
    }

    /// <summary>
    /// Displays basic database configuration information
    /// </summary>
    private void ShowDatabaseInfo()
    {
        _output.WriteLine("Database Configuration:", ConsoleColor.Blue);
        _output.WriteLine("=======================", ConsoleColor.Blue);
        _output.WriteLine($"Connection: {_settings.ConnectionString}");
    }

    /// <summary>
    /// Simulates database connection test with progress indicator
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    private async Task TestConnectionAsync(CancellationToken cancellationToken)
    {
        _output.WriteInfo("Testing database connection...");

        for (int i = 0; i <= 100; i += 10)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            Console.Write($"\rProgress: [{new string('#', i / 10)}{new string('-', 10 - i / 10)}] {i}%");
            await Task.Delay(500, cancellationToken);
        }

        Console.WriteLine(); // New line after progress
        _output.WriteSuccess("Database connection successful");
    }

    /// <summary>
    /// Displays detailed database settings
    /// </summary>
    private void ShowSettings()
    {
        _output.WriteLine("Database Settings:", ConsoleColor.Cyan);
        _output.WriteLine("==================", ConsoleColor.Cyan);
        _output.WriteLine($"Connection String: {_settings.ConnectionString}");
        _output.WriteLine($"Timeout: {_settings.Timeout}s");
        _output.WriteLine($"Logging Enabled: {(_settings.EnableLogging ? "Yes" : "No")}");
    }
}