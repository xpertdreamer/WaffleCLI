using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Core.TUI.Rendering;
using WaffleCLI.Core.TUI.Input;
using WaffleCLI.Core.TUI.Configuration;

namespace WaffleCLI.Core.TUI.Application
{
    /// <summary>
    /// DI service registration
    /// </summary>
    public static class ServiceRegistry
    {
        public static IServiceCollection AddTuiFramework(this IServiceCollection services)
        {
            services.AddSingleton<FocusManager>();
            services.AddSingleton<KeyBindingManager>();
            services.AddSingleton<ThemeManager>();
            services.AddSingleton<IRenderEngine, RenderEngine>();
            services.AddSingleton<IInputHandler, InputHandler>();
            services.AddSingleton<ITuiConfiguration, TuiConfiguration>();
            
            return services;
        }

        public static IServiceCollection AddTuiFramework(this IServiceCollection services, Action<TuiConfiguration> configure)
        {
            var config = new TuiConfiguration();
            configure(config);
            
            services.AddSingleton<ITuiConfiguration>(config);
            return AddTuiFramework(services);
        }
    }
}