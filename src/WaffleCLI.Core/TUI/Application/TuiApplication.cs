using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Application;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Exceptions;
using WaffleCLI.Core.TUI.Input;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

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
        private readonly ITuiConfiguration _configuration;
        private bool _isRunning = false;
        private readonly System.Diagnostics.Stopwatch _frameTimer;
        private readonly int _targetFrameTimeMs;

        public IComponent RootComponent { get; }
        public bool IsRunning => _isRunning;

        public TuiApplication(IServiceProvider serviceProvider, IComponent rootComponent, int targetFps = 60)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _renderEngine = serviceProvider.GetRequiredService<IRenderEngine>();
            _inputHandler = serviceProvider.GetRequiredService<IInputHandler>();
            _focusManager = serviceProvider.GetRequiredService<FocusManager>();
            _configuration = serviceProvider.GetService<ITuiConfiguration>() ?? new TuiConfiguration();
            RootComponent = rootComponent ?? throw new ArgumentNullException(nameof(rootComponent));
            
            _frameTimer = new System.Diagnostics.Stopwatch();
            _targetFrameTimeMs = Math.Max(1, 1000 / Math.Max(1, targetFps)); // Prevent division by zero
            
            RegisterFocusableComponents(rootComponent);
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
                    Render();
                    
                    // Frame rate limiting
                    int elapsed = (int)_frameTimer.ElapsedMilliseconds;
                    int sleepTime = Math.Max(0, _targetFrameTimeMs - elapsed);
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Application runtime error", ex);
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
                
                // Safe console dimension reading
                int width = 80;
                int height = 24;
                try
                {
                    width = Math.Max(1, Console.WindowWidth);
                    height = Math.Max(1, Console.WindowHeight);
                }
                catch
                {
                    // Use default dimensions if console is not available
                    TuiLogger.LogInfo("Using default console dimensions");
                }
                
                _renderEngine.Initialize(width, height);
                
                // Register global exit hotkey
                var keyBindingManager = _serviceProvider.GetRequiredService<KeyBindingManager>();
                keyBindingManager.RegisterGlobalHotkey(ConsoleKey.Escape, KeyModifiers.None, Stop);
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Failed to initialize TUI application", ex);
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
                TuiLogger.LogError("Input processing error", ex);
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
                TuiLogger.LogError("Update error", ex);
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
                TuiLogger.LogError("Render error", ex);
            }
        }

        private void Cleanup()
        {
            try
            {
                _inputHandler.Stop();
                Console.CursorVisible = true;
                Console.ResetColor();
                Console.Clear();
            }
            catch
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
            _renderEngine?.Dispose();
            RootComponent?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}