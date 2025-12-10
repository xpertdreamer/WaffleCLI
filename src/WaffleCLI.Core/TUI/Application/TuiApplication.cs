using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Application;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Exceptions;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Input;
using WaffleCLI.Core.TUI.Configuration;

namespace WaffleCLI.Core.TUI.Application
{
    /// <summary>
    /// Main TUI application implementation
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
        private int _lastWidth, _lastHeight;
        private int _framesSinceLastRender = 0;
        private const int FORCE_RENDER_INTERVAL = 2;

        /// <summary>
        /// Gets the root component
        /// </summary>
        public IComponent RootComponent { get; }
        
        /// <summary>
        /// Gets whether the application is running
        /// </summary>
        public bool IsRunning => _isRunning;

        private int _consecutiveIdleFrames = 0;
        private const int MAX_IDLE_FRAMES_BEFORE_SLOWDOWN = 10;
        private int _adaptiveTargetFrameTimeMs;

        // Modified constructor to initialize adaptive timing
        public TuiApplication(IServiceProvider serviceProvider, IComponent rootComponent, int targetFps = 60)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _renderEngine = serviceProvider.GetRequiredService<IRenderEngine>();
            _inputHandler = serviceProvider.GetRequiredService<IInputHandler>();
            _focusManager = serviceProvider.GetRequiredService<FocusManager>();
            _keyBindingManager = serviceProvider.GetRequiredService<KeyBindingManager>();
            _configuration = serviceProvider.GetService<ITuiConfiguration>() ?? new TuiConfiguration();
            RootComponent = rootComponent ?? throw new ArgumentNullException(nameof(rootComponent));

            // Get current console dimensions
            _lastWidth = Math.Max(40, Console.WindowWidth);
            _lastHeight = Math.Max(20, Console.WindowHeight);

            // Get window dimensions from settings if available (but don't resize console)
            var settingsManager = serviceProvider.GetService<SettingsManager>();
            if (settingsManager != null)
            {
                // Store settings but don't apply them to console
                var settings = settingsManager.Settings;
                Infrastructure.Logging.TuiLogger.LogInfo(
                    $"Settings dimensions: {settings.WindowWidth}x{settings.WindowHeight}");
            }

            _frameTimer = new System.Diagnostics.Stopwatch();
            _targetFrameTimeMs = 1000 / Math.Max(1, targetFps);
            _adaptiveTargetFrameTimeMs = _targetFrameTimeMs;

            RegisterFocusableComponents(rootComponent);
            _focusManager.FocusChanged += OnFocusChanged;
        }

        // Modified main loop to include adaptive frame rate
        private bool ShouldRenderThisFrame()
        {
            // Always render if there's input waiting
            if (Console.KeyAvailable)
            {
                _consecutiveIdleFrames = 0;
                return true;
            }

            // Force render periodically
            if (_framesSinceLastRender >= FORCE_RENDER_INTERVAL)
            {
                _consecutiveIdleFrames = 0;
                return true;
            }

            // Check for console resize
            if (CheckConsoleResize())
            {
                _consecutiveIdleFrames = 0;
                return true;
            }

            // Adaptive frame rate: reduce FPS when idle
            _consecutiveIdleFrames++;
            if (_consecutiveIdleFrames > MAX_IDLE_FRAMES_BEFORE_SLOWDOWN)
            {
                // When idle for a while, reduce frame rate to save CPU
                _adaptiveTargetFrameTimeMs = Math.Min(_targetFrameTimeMs * 4, 100); // Max 10 FPS when idle
            }
            else
            {
                _adaptiveTargetFrameTimeMs = _targetFrameTimeMs;
            }

            return false;
        }

        /// <summary>
        /// Starts the application main loop
        /// </summary>
        public void Run()
        {
            if (_isRunning) return;
    
            try
            {
                _isRunning = true;
                Initialize();
        
                while (_isRunning)
                {
                    _frameTimer.Restart();
            
                    ProcessInput();
                    Update();
            
                    bool shouldRender = _framesSinceLastRender >= FORCE_RENDER_INTERVAL || 
                                        Console.KeyAvailable || 
                                        CheckConsoleResize();
            
                    if (shouldRender)
                    {
                        Render();
                        _framesSinceLastRender = 0;
                    }
                    else
                    {
                        _framesSinceLastRender++;
                    }
            
                    int elapsed = (int)_frameTimer.ElapsedMilliseconds;
                    int sleepTime = _targetFrameTimeMs - elapsed;
            
                    if (sleepTime > 0)
                    {
                        // Use proper sleep with adaptive timing
                        if (sleepTime < 2)
                        {
                            // Very short sleep - use Thread.Yield for minimal delay
                            Thread.Yield();
                        }
                        else if (sleepTime < 15)
                        {
                            // Short sleep - use Thread.Sleep with minimal overhead
                            Thread.Sleep(1);
                        }
                        else
                        {
                            // Longer sleep - use Thread.Sleep with calculated time
                            Thread.Sleep(Math.Max(1, sleepTime - 1));
                        }
                    }
                    else
                    {
                        // Frame took too long - yield to prevent CPU hogging
                        Thread.Yield();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TuiException("Application runtime error", ex);
            }
            finally
            {
                Cleanup();
            }
        }

        /// <summary>
        /// Forces a refresh of the display
        /// </summary>
        public void Refresh()
        {
            Render();
        }

        private void Initialize()
        {
            try
            {
                Console.CursorVisible = false;
                Console.ResetColor();
                Console.Clear();
                
                // Initialize render engine with current console dimensions
                _renderEngine.Initialize(_lastWidth, _lastHeight);
                
                // Set root component dimensions
                RootComponent.Width = _lastWidth;
                RootComponent.Height = _lastHeight;
                
                // Call DoLayout for root component if it's a container
                if (RootComponent is IContainer container)
                {
                    container.DoLayout();
                }
                
                // Perform initial render
                _renderEngine.BeginFrame();
                RootComponent.Render(_renderEngine);
                _renderEngine.EndFrame();
                
                // Register global hotkeys
                _keyBindingManager.RegisterGlobalHotkey(ConsoleKey.Escape, KeyModifiers.None, Stop);
                
                Infrastructure.Logging.TuiLogger.LogInfo($"Application initialized with size: {_lastWidth}x{_lastHeight}");
            }
            catch (Exception ex)
            {
                throw new TuiException("Failed to initialize TUI application", ex);
            }
        }

        private void ProcessInput()
        {
            try
            {
                _inputHandler.ProcessInput();
            }
            catch (Exception ex)
            {
                // Input errors shouldn't crash the app
            }
        }

        private void Update()
        {
            try
            {
                RootComponent.Update();
            }
            catch (Exception ex)
            {
                // Update errors shouldn't crash the app
            }
        }

        private void Render()
        {
            try
            {
                _renderEngine.BeginFrame();
                RootComponent.Render(_renderEngine);
                _renderEngine.EndFrame();
            }
            catch (Exception ex)
            {
                // Render errors shouldn't crash the app
            }
        }

        private bool CheckConsoleResize()
        {
            try
            {
                int currentWidth = Console.WindowWidth;
                int currentHeight = Console.WindowHeight;
                
                if (currentWidth != _lastWidth || currentHeight != _lastHeight)
                {
                    _lastWidth = currentWidth;
                    _lastHeight = currentHeight;
                    
                    _renderEngine.Initialize(currentWidth, currentHeight);
                    
                    // Handle resize for any IContainer root component
                    if (RootComponent is IContainer container)
                    {
                        container.Width = currentWidth;
                        container.Height = currentHeight;
                        container.DoLayout();
                    }
                    else
                    {
                        // Fallback for non-container root components
                        RootComponent.Width = currentWidth;
                        RootComponent.Height = currentHeight;
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Ignore resize errors
            }
            
            return false;
        }

        private void OnFocusChanged(IFocusable? focusedComponent)
        {
            _framesSinceLastRender = FORCE_RENDER_INTERVAL;
        }

        private void PreciseSleep(int milliseconds)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
            {
                Thread.SpinWait(100);
            }
        }

        private void RegisterFocusableComponents(IComponent component)
        {
            if (component is IFocusable focusable)
            {
                _focusManager.RegisterFocusable(focusable);
            }
            
            foreach (var child in component.Children)
            {
                RegisterFocusableComponents(child);
            }
        }

        /// <summary>
        /// Stops the application
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Cleanly shuts down the application
        /// </summary>
        public void Shutdown()
        {
            Stop();
    
            // Wait a bit for the main loop to exit
            for (int i = 0; i < 10 && _isRunning; i++)
            {
                Thread.Sleep(10);
            }
    
            // Shutdown logging system
            Infrastructure.Logging.TuiLogger.Shutdown();
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
        
                // Shutdown logging
                Infrastructure.Logging.TuiLogger.Shutdown();
            }
            catch (Exception ex)
            {
                // Ignore cleanup errors, but log them
                System.Diagnostics.Debug.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes the application
        /// </summary>
        public void Dispose()
        {
            Shutdown();
            _focusManager.FocusChanged -= OnFocusChanged;
            _renderEngine?.Dispose();
            RootComponent?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}