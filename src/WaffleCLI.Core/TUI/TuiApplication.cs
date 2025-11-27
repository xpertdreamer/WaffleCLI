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
    private bool _needsRedraw = true;

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

        try
        {
            Console.WindowWidth = 80;
            Console.WindowHeight = 25;
            Console.BufferWidth = 80;
            Console.BufferHeight = 25;
        }
        catch
        {
            // ignored
        }
    }

    private static void CleanupConsole()
    {
        Console.ResetColor();
        Console.CursorVisible = true;
        Console.Clear();
    }

    private async Task MainLoop(CancellationToken cancellationToken)
    {
        var lastRenderTime = DateTime.Now;
        const int targetFps = 30;
        const double minFrameTime = 1000.0 / targetFps;
        
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            var currentTime = DateTime.Now;
            var elapsedTime = (currentTime - lastRenderTime).TotalMilliseconds;

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                await _currentScreen.HandleInputAsync(key);
                _needsRedraw = true;
            }

            if (_needsRedraw && elapsedTime >= minFrameTime)
            {
                await _currentScreen.RenderAsync();
                _needsRedraw = false;
                lastRenderTime = currentTime;
            }

            await Task.Delay(1, cancellationToken);
        }
    }

    public void RequestRedraw()
    {
        _needsRedraw = true;
    }
}