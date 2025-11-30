using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WaffleCLI.Core.TUI.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for DI container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTuiComponent<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.TryAddTransient<TInterface, TImplementation>();
            return services;
        }

        public static IServiceCollection AddTuiSingleton<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.TryAddSingleton<TInterface, TImplementation>();
            return services;
        }

        public static IServiceCollection AddTuiScoped<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.TryAddScoped<TInterface, TImplementation>();
            return services;
        }
    }
}