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

/// <summary>
/// Registers a strongly-typed configuration section as a singleton service in the dependency injection container.
/// </summary>
/// <typeparam name="T">The type of the configuration section class. Must be a class with a parameterless constructor.</typeparam>
/// <param name="services">The service collection to add the configuration section to.</param>
/// <param name="sectionName">The name of the configuration section to retrieve and register.</param>
/// <returns>The service collection for method chaining.</returns>
/// <remarks>
/// <para>
/// This extension method simplifies the registration of configuration sections by providing a strongly-typed
/// way to access configuration values throughout the application. The configuration section is registered
/// as a singleton and will be automatically populated with values from the underlying configuration source.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// services.ConfigureSection&lt;DatabaseOptions&gt;("Database");
/// services.ConfigureSection&lt;ApiSettings&gt;("Api");
/// </code>
/// </para>
/// <para>
/// The method resolves the <see cref="IConfigurationProvider"/> from the service provider at runtime
/// and uses it to retrieve the specified configuration section. The section is then available for
/// dependency injection in other parts of the application.
/// </para>
/// </remarks>
/// <example>
/// The following example shows how to use this method to register a database configuration section:
/// <code>
/// public class DatabaseOptions
/// {
///     public string ConnectionString { get; set; }
///     public int Timeout { get; set; }
/// }
/// 
/// // In Startup configuration:
/// services.ConfigureSection&lt;DatabaseOptions&gt;("Database");
/// 
/// // Then inject in a service:
/// public class UserService
/// {
///     public UserService(DatabaseOptions options) { ... }
/// }
/// </code>
/// </example>
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