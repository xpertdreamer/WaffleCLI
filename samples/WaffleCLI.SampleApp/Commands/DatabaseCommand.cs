using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using WaffleCLI.SampleApp.Models;

namespace WaffleCLI.SampleApp.Commands;

[Command("db", "Database operations")]
public class DatabaseCommand : ICommand
{
    private readonly DatabaseSettings _settings;
    private readonly IConsoleOutput _output;

    public DatabaseCommand(DatabaseSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    public string Name => "db";
    public string Description => "Database operations";

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

    private void ShowDatabaseInfo()
    {
        _output.WriteLine("Database Configuration:", ConsoleColor.Blue);
        _output.WriteLine("=======================", ConsoleColor.Blue);
        _output.WriteLine($"Connection: {_settings.ConnectionString}");
    }

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

    private void ShowSettings()
    {
        _output.WriteLine("Database Settings:", ConsoleColor.Cyan);
        _output.WriteLine("==================", ConsoleColor.Cyan);
        _output.WriteLine($"Connection String: {_settings.ConnectionString}");
        _output.WriteLine($"Timeout: {_settings.Timeout}s");
        _output.WriteLine($"Logging Enabled: {(_settings.EnableLogging ? "Yes" : "No")}");
    }
}