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
    /// High-performance TUI application with optimized rendering
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
        private int _targetFrameTimeMs;
        private int _lastWidth, _lastHeight;
        private bool _needsRender = true;
        private DateTime _lastInputTime = DateTime.Now;
        private const int FAST_RENDER_FPS = 60;
        private const int IDLE_RENDER_FPS = 10;
        private int _currentFps = FAST_RENDER_FPS;

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
            _targetFrameTimeMs = 1000 / Math.Max(1, targetFps);
            
            RegisterFocusableComponents(rootComponent);
            _focusManager.FocusChanged += OnFocusChanged;
        }

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
                    
                    // Smart rendering: only render when needed
                    if (_needsRender || HasRecentInput())
                    {
                        Render();
                        _needsRender = false;
                    }
                    
                    // Adaptive frame rate based on activity
                    AdjustFrameRate();
                    
                    // Efficient waiting
                    int elapsed = (int)_frameTimer.ElapsedMilliseconds;
                    int sleepTime = Math.Max(1, _targetFrameTimeMs - elapsed);
                    
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
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

        public void Stop()
        {
            _isRunning = false;
        }

        public void Refresh()
        {
            _needsRender = true;
        }

        private void Initialize()
        {
            try
            {
                Console.CursorVisible = false;
                Console.Clear();
                
                // Get console dimensions
                _lastWidth = Math.Max(40, Console.WindowWidth);
                _lastHeight = Math.Max(20, Console.WindowHeight - 1);
                
                _renderEngine.Initialize(_lastWidth, _lastHeight);
                
                // Register global hotkeys
                _keyBindingManager.RegisterGlobalHotkey(ConsoleKey.Escape, KeyModifiers.None, Stop);
                _keyBindingManager.RegisterGlobalHotkey(ConsoleKey.F5, KeyModifiers.None, () => {
                    _renderEngine.RequestFullRedraw();
                    _needsRender = true;
                });
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
                if (Console.KeyAvailable)
                {
                    _inputHandler.ProcessInput();
                    _lastInputTime = DateTime.Now;
                    _needsRender = true; // Input always requires render
                }
            }
            catch (Exception ex)
            {
                // Log input errors but don't crash
            }
        }

        private void Update()
        {
            try
            {
                // Check for console resize
                if (CheckConsoleResize())
                {
                    _needsRender = true;
                }
                
                RootComponent.Update();
            }
            catch (Exception ex)
            {
                // Log update errors but don't crash
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
                // Log render errors but don't crash
            }
        }

        private bool CheckConsoleResize()
        {
            try
            {
                int currentWidth = Console.WindowWidth;
                int currentHeight = Console.WindowHeight - 1;
                
                if (currentWidth != _lastWidth || currentHeight != _lastHeight)
                {
                    _lastWidth = currentWidth;
                    _lastHeight = currentHeight;
                    
                    _renderEngine.Initialize(currentWidth, currentHeight);
                    
                    if (RootComponent is WaffleCLI.Core.TUI.Components.Primitive.Panel panel)
                    {
                        panel.Width = currentWidth;
                        panel.Height = currentHeight;
                        panel.DoLayout();
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
            _needsRender = true; // Focus changes require re-render
        }

        private bool HasRecentInput()
        {
            // Consider input "recent" if it happened in the last 100ms
            return (DateTime.Now - _lastInputTime).TotalMilliseconds < 100;
        }

        private void AdjustFrameRate()
        {
            // Adaptive frame rate: fast when active, slow when idle
            int newFps = HasRecentInput() || _needsRender ? FAST_RENDER_FPS : IDLE_RENDER_FPS;
            
            if (newFps != _currentFps)
            {
                _currentFps = newFps;
                _targetFrameTimeMs = 1000 / _currentFps;
            }
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
            }
            catch (Exception ex)
            {
                // Ignore cleanup errors
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

        public void Dispose()
        {
            _focusManager.FocusChanged -= OnFocusChanged;
            _renderEngine?.Dispose();
            RootComponent?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}