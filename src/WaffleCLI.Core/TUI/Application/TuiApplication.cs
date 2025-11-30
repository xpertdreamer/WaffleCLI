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
    /// Optimized TUI application with minimal overhead
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
        private const int FORCE_RENDER_INTERVAL = 2; // Force render every 3rd frame

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
                    
                    // Smart rendering: only render when necessary
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
                    
                    // Efficient frame rate limiting
                    int elapsed = (int)_frameTimer.ElapsedMilliseconds;
                    int sleepTime = _targetFrameTimeMs - elapsed;
                    
                    if (sleepTime > 0)
                    {
                        // Use precise sleep for small intervals, Thread.Sleep for larger
                        if (sleepTime < 15)
                        {
                            PreciseSleep(sleepTime);
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
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
            Render();
        }

        private void Initialize()
        {
            try
            {
                Console.CursorVisible = false;
                Console.Clear();
                
                _lastWidth = Math.Max(40, Console.WindowWidth);
                _lastHeight = Math.Max(20, Console.WindowHeight - 1);
                
                _renderEngine.Initialize(_lastWidth, _lastHeight);
                
                _keyBindingManager.RegisterGlobalHotkey(ConsoleKey.Escape, KeyModifiers.None, Stop);
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
            // Force render on focus change for immediate visual feedback
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