using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Configuration;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Options;
using WaffleCLI.Runtime.Output;
using WaffleCLI.Runtime.Services;
using IConfigurationProvider = WaffleCLI.Configuration.IConfigurationProvider;

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
        services.TryAddSingleton<ICommandExecutor, CommandExecutor>();
        services.TryAddSingleton<IConsoleHost, DefaultConsoleHost>();
        services.TryAddSingleton<IConfigurationProvider, JsonConfigurationProvider>();
        
        // Register console output service
        services.TryAddSingleton<IConsoleOutput, DefaultConsoleOutput>();
        
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
        // Register command in DI container
        services.AddTransient<TCommand>();
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

    /// <summary>
    /// Registers a strongly-typed configuration section as a singleton service in the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The type of the configuration section class. Must be a class with a parameterless constructor.</typeparam>
    /// <param name="services">The service collection to add the configuration section to.</param>
    /// <param name="sectionName">The name of the configuration section to retrieve and register.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection ConfigureSection<T>(this IServiceCollection services, string sectionName)
        where T : class, new()
    {
        services.AddSingleton(provider =>
        {
            var configProvider = provider.GetRequiredService<IConfigurationProvider>();
            return configProvider.GetSection<T>(sectionName);
        });
        return services;
    }
}