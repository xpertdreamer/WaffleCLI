using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Application;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Core.TUI.Rendering;
using WaffleCLI.Core.TUI.Input;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Application
{
    /// <summary>
    /// Fluent builder for TUI applications with enhanced configuration
    /// </summary>
    public class TuiApplicationBuilder : ITuiApplicationBuilder
    {
        private readonly IServiceCollection _services;
        private Type? _rootComponentType;
        private Func<IServiceProvider, IComponent>? _rootComponentFactory;
        private readonly ApplicationConfiguration _config = new();
        
        private class ApplicationConfiguration : ITuiApplicationConfiguration
        {
            public int FrameRate { get; set; } = 60;
            public string Theme { get; set; } = "default";
            public bool EnableDoubleBuffering { get; set; } = true;
            public bool EnableInputLogging { get; set; } = false;
            public int InitialWidth { get; set; } = 120;
            public int InitialHeight { get; set; } = 35;
        }

        public TuiApplicationBuilder()
        {
            _services = new ServiceCollection();
            ConfigureDefaultServices();
        }

        private void ConfigureDefaultServices()
        {
            // Register core framework services
            _services.AddSingleton<FocusManager>();
            _services.AddSingleton<KeyBindingManager>();
            _services.AddSingleton<ThemeManager>();
            _services.AddSingleton<BinariesManager>();
            _services.AddSingleton<IRenderEngine, RenderEngine>();
            _services.AddSingleton<IInputHandler, InputHandler>();
            _services.AddSingleton<ITuiConfiguration, TuiConfiguration>();
            _services.AddSingleton<ITuiApplicationConfiguration>(_config);
            
            TuiLogger.LogInfo("Default services configured");
        }

        public ITuiApplicationBuilder UseRootComponent<T>() where T : class, IComponent
        {
            _rootComponentType = typeof(T);
            
            // Register the root component as transient to avoid disposal issues
            _services.AddTransient<T>();
            _services.AddTransient<IComponent, T>(provider => provider.GetRequiredService<T>());
            
            TuiLogger.LogInfo($"Registered root component: {_rootComponentType.Name}");
            
            return this;
        }

        public ITuiApplicationBuilder UseRootComponent(Func<IServiceProvider, IComponent> factory)
        {
            _rootComponentFactory = factory;
            
            _services.AddTransient<IComponent>(provider => factory(provider));
            
            TuiLogger.LogInfo($"Registered root component factory");
            
            return this;
        }

        public ITuiApplicationBuilder ConfigureServices(Action<IServiceCollection> configureAction)
        {
            configureAction(_services);
            TuiLogger.LogInfo("Custom services configuration applied");
            return this;
        }

        public ITuiApplicationBuilder WithFrameRate(int frameRate)
        {
            _config.FrameRate = Math.Clamp(frameRate, 1, 120);
            TuiLogger.LogInfo($"Set frame rate to: {_config.FrameRate} FPS");
            return this;
        }

        public ITuiApplicationBuilder WithTheme(string themeName)
        {
            _config.Theme = themeName;
            TuiLogger.LogInfo($"Set theme to: {themeName}");
            return this;
        }

        public ITuiApplicationBuilder EnableDoubleBuffering(bool enable = true)
        {
            _config.EnableDoubleBuffering = enable;
            TuiLogger.LogInfo($"Double buffering: {(enable ? "ENABLED" : "DISABLED")}");
            return this;
        }

        public ITuiApplicationBuilder EnableInputLogging(bool enable = true)
        {
            _config.EnableInputLogging = enable;
            TuiLogger.LogInfo($"Input logging: {(enable ? "ENABLED" : "DISABLED")}");
            return this;
        }

        public ITuiApplication Build()
        {
            if (_rootComponentType == null && _rootComponentFactory == null)
            {
                throw new InvalidOperationException("Root component must be specified using UseRootComponent<T>() or UseRootComponent(factory)");
            }

            var serviceProvider = _services.BuildServiceProvider();
            
            // Resolve root component
            IComponent rootComponent;
            if (_rootComponentFactory != null)
            {
                rootComponent = _rootComponentFactory(serviceProvider);
            }
            else
            {
                rootComponent = serviceProvider.GetService(_rootComponentType!) as IComponent;
            }
            
            if (rootComponent == null)
            {
                throw new InvalidOperationException($"Failed to resolve root component");
            }
            
            TuiLogger.LogInfo("TuiApplication built successfully");
            
            return new TuiApplication(serviceProvider, rootComponent, _config.FrameRate);
        }
    }
}