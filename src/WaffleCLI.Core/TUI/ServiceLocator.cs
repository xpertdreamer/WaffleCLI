using Microsoft.Extensions.DependencyInjection;

namespace WaffleCLI.Core.TUI;

public static class ServiceLocator
{
    private static IServiceProvider _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static T GetService<T>() where T : class
    {
        return _serviceProvider == null ? throw new InvalidOperationException("ServiceLocator has not been initialized. Call Initialize first.") : _serviceProvider.GetRequiredService<T>();
    }

    public static object GetService(Type serviceType)
    {
        return _serviceProvider == null ? throw new InvalidOperationException("ServiceLocator has not been initialized. Call Initialize first.") : _serviceProvider.GetRequiredService(serviceType);
    }
}