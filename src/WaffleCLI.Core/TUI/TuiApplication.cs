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
    private (int width, int height) _lastConsoleSize;

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
            
            _lastConsoleSize = (Console.WindowWidth, Console.WindowHeight);

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
        Console.Title = "WaffleTUI Application";

        try
        {
            Console.WindowWidth = Math.Max(80, Console.WindowHeight);
            Console.WindowHeight = Math.Max(25, Console.WindowHeight);
            #if WIN32
                Console.BufferWidth = Console.WindowWidth;
                Console.BufferHeight = Console.WindowHeight;
            #endif
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
        const int targetFps = 60;
        const double minFrameTime = 1000.0 / targetFps;
        
        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            var currentSize = (Console.WindowWidth, Console.WindowHeight);
            if (currentSize != _lastConsoleSize)
            {
                _lastConsoleSize = currentSize;
                await _currentScreen.HandleResizeAsync();
                _needsRedraw = true;
            }
            
            var currentTime = DateTime.Now;
            var elapsed = (currentTime - lastRenderTime).TotalMilliseconds;

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                await _currentScreen.HandleInputAsync(key);
                _needsRedraw = true;
            }

            if (_needsRedraw && elapsed >= minFrameTime)
            {
                await _currentScreen.RenderAsync();
                _needsRedraw = false;
                lastRenderTime = currentTime;
            }
            if (!_needsRedraw)
            {
                await Task.Delay(1, cancellationToken);
            }
        }
    }

    public void RequestRedraw()
    {
        _needsRedraw = true;
    }
}