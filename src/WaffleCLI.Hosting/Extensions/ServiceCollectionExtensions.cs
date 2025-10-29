using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Configuration;
using WaffleCLI.Runtime.Options;
using WaffleCLI.Runtime.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WaffleCLI.Hosting.Extensions;

/// <summary>
/// Extension methods for setting up console framework services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the console framework services to the specified service collection
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddWaffleCli(this IServiceCollection services)
    {
        services.AddOptions();
        
        // Register core services
        services.TryAddSingleton<ICommandRegistry, CommandRegistry>();
        services.TryAddSingleton<ICommandRegistry, CommandRegistry>();
        services.TryAddSingleton<ICommandExecutor, CommandExecutor>();
        services.TryAddSingleton<IConsoleHost, DefaultConsoleHost>();
        services.TryAddSingleton<IConfigurationProvider, JsonConfigurationProvider>();
        
        // Configure default options
        services.Configure<ConsoleHostOptions>(options => { });
        services.Configure<CommandExecutorOptions>(options => { });
        return services;
    }

    /// <summary>
    /// Adds a command to the service collection
    /// </summary>
    /// <typeparam name="TCommand">The type of command to add</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services) where TCommand : class, ICommand
    {
        services.AddTransient<TCommand>();
        services.AddTransient<TCommand, TCommand>(provider => provider.GetRequiredService<TCommand>());
        return services;
    }
    
    /// <summary>
    /// Adds JSON configuration support
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="filePath">The configuration file path</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services,
        string filePath = "appsettings.json")
    {
        services.RemoveAll<IConfigurationProvider>();
        services.AddSingleton<IConfigurationProvider>(_ => new JsonConfigurationProvider(filePath));
        return services;
    }
}