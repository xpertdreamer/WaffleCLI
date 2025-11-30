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

namespace WaffleCLI.Core.TUI.Application
{
    /// <summary>
    /// Fluent builder for TUI applications
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
            _services.AddSingleton<IInputHandler, InputHandler>();
            _services.AddSingleton<ITuiConfiguration, TuiConfiguration>();
        }

        public ITuiApplicationBuilder UseRootComponent<T>() where T : class, IComponent
        {
            _rootComponentType = typeof(T);
            
            // Register the root component as both the concrete type and IComponent
            _services.AddSingleton<T>();
            _services.AddSingleton<IComponent>(provider => provider.GetRequiredService<T>());
            
            return this;
        }

        public ITuiApplicationBuilder ConfigureServices(Action<IServiceCollection> configureAction)
        {
            configureAction(_services);
            return this;
        }

        public ITuiApplication Build()
        {
            if (_rootComponentType == null)
            {
                throw new InvalidOperationException("Root component must be specified using UseRootComponent<T>()");
            }

            var serviceProvider = _services.BuildServiceProvider();
            
            // Get the root component - now it should be properly registered
            var rootComponent = serviceProvider.GetRequiredService(_rootComponentType) as IComponent;
            if (rootComponent == null)
            {
                throw new InvalidOperationException($"Failed to resolve root component of type {_rootComponentType.Name}");
            }
            
            return new TuiApplication(serviceProvider, rootComponent);
        }
    }
}