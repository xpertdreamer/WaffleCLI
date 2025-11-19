using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Runtime.TUI.Screens;

namespace WaffleCLI.Runtime.TUI;

/// <summary>
/// Provides a console-based Text User Interface (TUI) application implementation.
/// </summary>
/// <remarks>
/// Manages the TUI application lifecycle, screen navigation, and input handling.
/// Supports smooth rendering, keyboard input processing, and proper console cleanup on exit.
/// </remarks>
public class ConsoleTuiApplication : ITuiApplication
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConsoleTuiApplication> _logger;
    private bool _isRunning;
    private ITuiScreen? _currentScreen;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleTuiApplication"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving TUI screens and dependencies.</param>
    /// <param name="logger">The logger for recording application events and errors.</param>
    public ConsoleTuiApplication(IServiceProvider serviceProvider, ILogger<ConsoleTuiApplication> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Runs the TUI application asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop the application.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Initializes the console environment, starts the main screen, and enters the main rendering loop.
    /// The loop handles keyboard input and renders the current screen at approximately 20 FPS.
    /// Ensures proper console cleanup on exit or error.
    /// </remarks>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            return;
        
        _isRunning = true;
        _logger.LogInformation("Starting TUI application");

        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = true;
            Console.Clear();

            _currentScreen = _serviceProvider.GetRequiredService<MainScreen>();
            await _currentScreen.InitializeAsync();

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                await _currentScreen.RenderAsync();

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    await _currentScreen.HandleKeyAsync(key);
                }

                await Task.Delay(50, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TUI application failed");
        }
        finally
        {
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.Clear();
            _isRunning = false;
        }
    }

    /// <summary>
    /// Stops the TUI application asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    /// Signals the application to exit gracefully on the next iteration of the main loop.
    /// </remarks>
    public Task StopAsync()
    {
        _isRunning = false;
        return Task.CompletedTask;
    }
}