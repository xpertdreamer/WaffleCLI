using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Elements;
using WaffleCLI.Core.TUI.Rendering;

namespace WaffleCLI.Core.TUI;

public class NewTuiApplication : ITuiApplication
{
    private readonly IServiceProvider _services;
    private readonly ILogger<NewTuiApplication> _logger;
    private readonly ConfigurationManager _configManager;
    private readonly IRenderEngine _renderEngine;
    private readonly RenderLayerManager _layerManager;

    private ITuiScreen _currentScreen = null!;
    private bool _isRunning;
    private bool _needsRedraw = true;
    private (int width, int height) _lastSize;
    private Stopwatch _frameStopwatch = new();
    private double _frameTime;
    private bool _showRenderStats;

    public NewTuiApplication(IServiceProvider services, ILogger<NewTuiApplication> logger,
        ConfigurationManager configManager, IRenderEngine renderEngine, RenderLayerManager layerManager)
    {
        _services = services;
        _logger = logger;
        _configManager = configManager;
        _renderEngine = renderEngine;
        _layerManager = layerManager;

        _configManager.ConfigChanged += OnConfigChanged;
        _showRenderStats = _configManager.Config.Rendering.ShowRenderStats;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;
        
        _isRunning = true;
        _logger.LogInformation("Starting new TUI application");

        try
        {
            SetupConsole();
            SetupLayers();

            _currentScreen = _services.GetRequiredService<ITuiScreen>();
            await _currentScreen.InitializeAsync();

            await MainLoop(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "New TUI application failed");
            throw;
        }
        finally
        {
            CleanupConsole();
            _isRunning = false;
        }
    }

    private void SetupConsole()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Title = _configManager.Config.Window.Title;
        
        try
        {
            Console.WindowWidth = _configManager.Config.Window.Width;
            Console.WindowHeight = _configManager.Config.Window.Height;
            Console.BufferWidth = _configManager.Config.Window.Width;
            Console.BufferHeight = _configManager.Config.Window.Height;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set console dimensions");
        }

        _lastSize = (Console.WindowWidth, Console.WindowHeight);
        _renderEngine.Initialize(_lastSize.width, _lastSize.height);
        
        _renderEngine.Clear();
        _renderEngine.Flush();
    }

    private void SetupLayers()
    {
        _layerManager.AddLayer("background", 0);
        _layerManager.AddLayer("content", 1);
        _layerManager.AddLayer("overlay", 2);
        _layerManager.AddLayer("debug", 3, _showRenderStats);
    }

    private async Task MainLoop(CancellationToken cancellationToken)
    {
        var targetFrameTime = 1000.0 / _configManager.Config.Rendering.TargetFps;

        while (!cancellationToken.IsCancellationRequested && _isRunning)
        {
            _frameStopwatch.Restart();

            var currentSize = (Console.WindowWidth, Console.WindowHeight);
            if (currentSize != _lastSize)
            {
                _lastSize = currentSize;
                _renderEngine.Initialize(currentSize.WindowWidth, currentSize.WindowHeight);
                await _currentScreen.HandleResizeAsync();
                _needsRedraw = true;
            }

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                await HandleGlobalInput(key);
                await _currentScreen.HandleInputAsync(key);
                _needsRedraw = true;
            }

            if (_needsRedraw)
            {
                await RenderFrame();
                _needsRedraw = false;
            }

            if (_configManager.Config.Rendering.VSync)
            {
                var elapsed = _frameStopwatch.ElapsedMilliseconds;
                if (elapsed < targetFrameTime)
                {
                    await Task.Delay((int)(targetFrameTime - elapsed), cancellationToken);  
                }
            }

            _frameTime = _frameStopwatch.Elapsed.TotalMilliseconds;
        }
    }

    private async Task HandleGlobalInput(ConsoleKeyInfo keyInfo)
    {
        var keyBindings = _configManager.Config.Input.KeyBindings;

        if (IsKeyBinding(keyInfo, keyBindings.ToggleStats))
        {
            _showRenderStats = !_showRenderStats;
            _layerManager.GetLayers().First(l => l.Name == "debug").IsVisible = _showRenderStats;
        } else if (IsKeyBinding(keyInfo, keyBindings.Exit))
        {
            if (_configManager.Config.Behavior.ConfirmExit)
            {
                await ShowExitConfirmation();
            }
            else
            {
                await StopAsync();
            }
        }
        else if (IsKeyBinding(keyInfo, keyBindings.Screenshot))
        {
            await TakeScreenshot();
        }
    }

    private bool IsKeyBinding(ConsoleKeyInfo keyInfo, string binding)
    {
        var parts = binding.Split('+');
        var keyPart = parts.Last();
        var modifiers = parts.Take(parts.Length - 1);

        if (!Enum.TryParse<ConsoleKey>(keyPart, true, out var key)) return false;
        if (keyInfo.Key != key) return false;
        
        var ctrl = modifiers.Contains("Ctrl");
        var shift = modifiers.Contains("Shift");
        var alt = modifiers.Contains("Alt");
        
        return keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) == ctrl
            && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift) == shift
            && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt) == alt;
    }

    private async Task RenderFrame()
    {
        try
        {
            _renderEngine.BeginFrame();

            var config = _configManager.Config;
            var theme = config.Theme.Themes[config.Theme.Current];
            var bgColor = ParseColor(theme.Colors.Background);
            _renderEngine.RenderRect(0, 0, _renderEngine.Width, _renderEngine.Height, bgColor, ' ');

            _layerManager.RenderAllLayers();

            if (_showRenderStats)
                RenderStatsOverlay();

            _renderEngine.EndFrame();
            _renderEngine.Flush();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during frame rendering");
            _needsRedraw = true;
        }
    }
    
    private ConsoleColor ParseColor(string colorName)
    {
        return Enum.TryParse<ConsoleColor>(colorName, true, out var color) 
            ? color 
            : ConsoleColor.Black;
    }

    private void RenderStatsOverlay()
    {
        var stats = _renderEngine is DoubleBufferedRenderEngine doubleBuffered 
            ? doubleBuffered.LastRenderStats 
            : new RenderStats(0, 0, 0, 0, 0);

        var statsText = new[]
        {
            $"FPS: {1000 / _frameTime:0}",
            $"Frame: {_frameTime:0.00}ms",
            $"Render: {stats.RenderTimeMs:0.00}ms",
            $"Flush: {stats.FlushTimeMs:0.00}ms",
            $"Elements: {stats.ElementRendered}",
            $"Dirty: {stats.DirtyRegion} regions",
            $"Screen: {Console.WindowWidth}x{Console.WindowHeight}",
            $"Buffer: {_renderEngine.Width}x{_renderEngine.Height}"
        };

        for (var i = 0; i < statsText.Length; i++)
        {
            _renderEngine.RenderText(2, 2 + i, statsText[i], ConsoleColor.White, ConsoleColor.DarkBlue);
        }
    }
    
    private void OnConfigChanged(AppConfig newConfig)
    {
        _logger.LogInformation("Configuration changed, applying updates");
        _showRenderStats = newConfig.Rendering.ShowRenderStats;
        _needsRedraw = true;
    }

    private async Task ShowExitConfirmation()
    {
        var overlayElement = new TextElement
        {
            X = 10,
            Y = 10,
            Width = 20,
            Height = 3,
            Text = " Exit? (y/n) ",
            Color = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkRed,
            HasBorder = true,
            isVisible = true,
            isFocusable = false
        };

        _layerManager.AddElementsToLayer("overlay", overlayElement);
        _needsRedraw = true;
        _renderEngine.Flush();

        var key = Console.ReadKey(true);
        
        _layerManager.RemoveElementFromLayer("overlay", overlayElement);
        _needsRedraw = true;

        if (key.Key == ConsoleKey.Y)
        {
            await StopAsync();
        }
    }

    private async Task TakeScreenshot()
    {
        try
        {
            var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"screenshot_{timeStamp}.ansi";
            _logger.LogInformation("Screenshot saved as {Filename}", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save screenshot");
        }
    }

    public Task StopAsync()
    {
        _isRunning = false;
        return Task.CompletedTask;
    }

    private void CleanupConsole()
    {
        Console.ResetColor();
        Console.CursorVisible = true;
        Console.Clear();
        if (_configManager.Config.Behavior.AutoSave)
            _configManager.SaveConfiguration();
    }

    public void Dispose()
    {
        _configManager?.Dispose();
    }
}