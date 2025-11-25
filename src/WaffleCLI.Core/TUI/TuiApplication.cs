using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI;

public class TuiApplication : ITuiApplication
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TuiApplication> _logger;
    private ITuiScreen _currentScreen =  null!;
    private bool _isRunning;

    public TuiApplication(IServiceProvider serviceProvider, ILogger<TuiApplication> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;

        _isRunning = true;
        _logger.LogInformation("Starting TuiApplication");

        try
        {
            SetupConsole();

            _currentScreen = _serviceProvider.GetRequiredService<ITuiScreen>();
            await _currentScreen.InitializeAsync();

            await MainLoop(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TuiApplication");
            throw;
        }
        finally
        {
            CleanupConsole();
            _isRunning = false;
        }
    }

    public Task StopAsync()
    {
        _isRunning = false;
        return Task.CompletedTask;
    }

    private static void SetupConsole()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Clear();
    }

    private static void CleanupConsole()
    {
        Console.ResetColor();
        Console.CursorVisible = true;
        Console.Clear();
    }

    private async Task MainLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            await _currentScreen.RenderAsync();

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                await _currentScreen.HandleInputAsync(key);
            }

            await Task.Delay(5, cancellationToken);
        }
    }
}