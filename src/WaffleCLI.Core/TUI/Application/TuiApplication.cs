using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Application;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Exceptions;
using WaffleCLI.Core.TUI.Input;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Configuration;

namespace WaffleCLI.Core.TUI.Application
{
    /// <summary>
    /// Enhanced TUI application with forced redraw on interaction
    /// </summary>
    public class TuiApplication : ITuiApplication
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRenderEngine _renderEngine;
        private readonly IInputHandler _inputHandler;
        private readonly FocusManager _focusManager;
        private readonly KeyBindingManager _keyBindingManager;
        private readonly ITuiConfiguration _configuration;
        private bool _isRunning = false;
        private readonly System.Diagnostics.Stopwatch _frameTimer;
        private readonly int _targetFrameTimeMs;
        private int _frameCount = 0;
        private int _lastWidth, _lastHeight;
        private bool _forceRedraw = true;
        private int _pendingRedraws = 0;

        public IComponent RootComponent { get; }
        public bool IsRunning => _isRunning;

        public TuiApplication(IServiceProvider serviceProvider, IComponent rootComponent, int targetFps = 60)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _renderEngine = serviceProvider.GetRequiredService<IRenderEngine>();
            _inputHandler = serviceProvider.GetRequiredService<IInputHandler>();
            _focusManager = serviceProvider.GetRequiredService<FocusManager>();
            _keyBindingManager = serviceProvider.GetRequiredService<KeyBindingManager>();
            _configuration = serviceProvider.GetService<ITuiConfiguration>() ?? new TuiConfiguration();
            RootComponent = rootComponent ?? throw new ArgumentNullException(nameof(rootComponent));
            
            _frameTimer = new System.Diagnostics.Stopwatch();
            _targetFrameTimeMs = Math.Min(16, 1000 / Math.Max(1, targetFps));
            
            RegisterFocusableComponents(rootComponent);
            
            // Subscribe to focus changes to force redraw
            _focusManager.FocusChanged += OnFocusChanged;
            
            // Register component interaction handler
            RegisterInteractionHandlers();
        }

        public void Run()
        {
            if (_isRunning) return;
            
            try
            {
                _isRunning = true;
                Initialize();
                
                Infrastructure.Logging.TuiLogger.LogInfo("Starting main application loop");
                
                while (_isRunning)
                {
                    _frameTimer.Restart();
                    _frameCount++;
                    
                    ProcessInput();
                    Update();
                    Render();
                    
                    // Proper frame rate limiting
                    int elapsed = (int)_frameTimer.ElapsedMilliseconds;
                    int sleepTime = Math.Max(1, _targetFrameTimeMs - elapsed);
                    
                    // Log frame rate every 5 seconds
                    if (_frameCount % (60 * 5) == 0)
                    {
                        double fps = 1000.0 / elapsed;
                        Infrastructure.Logging.TuiLogger.LogInfo($"Application running - FPS: {fps:F1}, Frame time: {elapsed}ms, Pending redraws: {_pendingRedraws}");
                    }
                    
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Application runtime error", ex);
                throw new TuiException("Application runtime error", ex);
            }
            finally
            {
                Cleanup();
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                Infrastructure.Logging.TuiLogger.LogInfo("Stopping application");
                _isRunning = false;
            }
        }

        public void Refresh()
        {
            _forceRedraw = true;
            _pendingRedraws++;
            if (_isRunning)
            {
                Render();
            }
        }

        private void Initialize()
        {
            try
            {
                Console.CursorVisible = false;
                Console.Clear();
                
                // Get console dimensions safely without causing scroll
                _lastWidth = 80;
                _lastHeight = 24;
                try
                {
                    _lastWidth = Math.Max(40, Console.WindowWidth);
                    _lastHeight = Math.Max(20, Console.WindowHeight - 1); // Reserve one line to prevent scrolling
                    Infrastructure.Logging.TuiLogger.LogInfo($"Detected console dimensions: {_lastWidth}x{_lastHeight}");
                }
                catch (Exception ex)
                {
                    Infrastructure.Logging.TuiLogger.LogWarning($"Failed to read console dimensions, using defaults - {ex}");
                }
                
                // Initialize render engine with safe dimensions
                _renderEngine.Initialize(_lastWidth, _lastHeight);
                
                // Register global exit hotkey
                _keyBindingManager.RegisterGlobalHotkey(ConsoleKey.Escape, KeyModifiers.None, Stop);
                
                // Register refresh hotkey for debugging
                _keyBindingManager.RegisterGlobalHotkey(ConsoleKey.F5, KeyModifiers.None, () => {
                    Infrastructure.Logging.TuiLogger.LogInfo("Manual refresh triggered by F5");
                    _forceRedraw = true;
                    _pendingRedraws++;
                });
                
                Infrastructure.Logging.TuiLogger.LogInfo("TUI application initialized successfully");
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Failed to initialize TUI application", ex);
                throw new TuiException("Failed to initialize TUI application", ex);
            }
        }

        private void ProcessInput()
        {
            try
            {
                _inputHandler.ProcessInput();
                
                // After processing input, check if we need to force a redraw
                if (_pendingRedraws > 0)
                {
                    _forceRedraw = true;
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Input processing error", ex);
            }
        }

        private void Update()
        {
            try
            {
                // Check for console resize
                if (CheckConsoleResize())
                {
                    _forceRedraw = true;
                    _pendingRedraws++;
                }
                
                RootComponent.Update();
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Update error", ex);
            }
        }

        private void Render()
        {
            try
            {
                _renderEngine.BeginFrame();
                RootComponent.Render(_renderEngine);
                _renderEngine.EndFrame();
                
                // Reset flags after successful render
                if (_forceRedraw)
                {
                    _pendingRedraws = Math.Max(0, _pendingRedraws - 1);
                    _forceRedraw = _pendingRedraws > 0;
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Render error", ex);
            }
        }

        private bool CheckConsoleResize()
        {
            try
            {
                int currentWidth = Console.WindowWidth;
                int currentHeight = Console.WindowHeight - 1; // Reserve one line
                
                if (currentWidth != _lastWidth || currentHeight != _lastHeight)
                {
                    Infrastructure.Logging.TuiLogger.LogInfo($"Console resized: {_lastWidth}x{_lastHeight} -> {currentWidth}x{currentHeight}");
                    
                    _lastWidth = currentWidth;
                    _lastHeight = currentHeight;
                    
                    // Reinitialize render engine with new dimensions
                    _renderEngine.Initialize(currentWidth, currentHeight);
                    
                    // Update root component dimensions
                    if (RootComponent is WaffleCLI.Core.TUI.Components.Primitive.Panel panel)
                    {
                        panel.Width = currentWidth;
                        panel.Height = currentHeight;
                        panel.DoLayout(); // Force layout update
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Error checking console resize - {ex}");
            }
            
            return false;
        }

        private void OnFocusChanged(IFocusable? focusedComponent)
        {
            // Force redraw when focus changes to ensure visual updates
            _forceRedraw = true;
            _pendingRedraws++;
            Infrastructure.Logging.TuiLogger.LogDebug($"Focus changed, forcing redraw. New focus: {focusedComponent?.Id ?? "None"}");
        }

        private void RegisterInteractionHandlers()
        {
            // This method would set up event handlers for component interactions
            // For now, we'll rely on the focus change events and input processing
        }

        private void Cleanup()
        {
            try
            {
                _focusManager.FocusChanged -= OnFocusChanged;
                _inputHandler.Stop();
                Console.CursorVisible = true;
                Console.ResetColor();
                Console.Clear();
                Infrastructure.Logging.TuiLogger.LogInfo("Application cleanup completed");
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Cleanup error", ex);
            }
        }

        private void RegisterFocusableComponents(IComponent component)
        {
            if (component is IFocusable focusable)
            {
                _focusManager.RegisterFocusable(focusable);
                Infrastructure.Logging.TuiLogger.LogDebug($"Registered focusable component: {component.Id}");
            }
            
            foreach (var child in component.Children)
            {
                RegisterFocusableComponents(child);
            }
        }

        public void Dispose()
        {
            _focusManager.FocusChanged -= OnFocusChanged;
            _renderEngine?.Dispose();
            RootComponent?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}