using System;
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
    /// Fluent builder for TUI applications with proper DI registration
    /// </summary>
    public class TuiApplicationBuilder : ITuiApplicationBuilder
    {
        private readonly IServiceCollection _services;
        private Type? _rootComponentType;

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
            _services.AddSingleton<IRenderEngine, RenderEngine>();
            // _services.AddSingleton<IRenderEngine, EnhancedRenderEngine>();
            _services.AddSingleton<IInputHandler, InputHandler>();
            _services.AddSingleton<ITuiConfiguration, TuiConfiguration>();
            
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

        public ITuiApplicationBuilder ConfigureServices(Action<IServiceCollection> configureAction)
        {
            configureAction(_services);
            TuiLogger.LogInfo("Custom services configuration applied");
            return this;
        }

        public ITuiApplication Build()
        {
            if (_rootComponentType == null)
            {
                throw new InvalidOperationException("Root component must be specified using UseRootComponent<T>()");
            }

            var serviceProvider = _services.BuildServiceProvider();
            
            // Validate that root component can be resolved
            var rootComponent = serviceProvider.GetService(_rootComponentType) as IComponent;
            if (rootComponent == null)
            {
                throw new InvalidOperationException($"Failed to resolve root component of type {_rootComponentType.Name}");
            }
            
            TuiLogger.LogInfo("TuiApplication built successfully");
            
            return new TuiApplication(serviceProvider, rootComponent);
        }
    }
}