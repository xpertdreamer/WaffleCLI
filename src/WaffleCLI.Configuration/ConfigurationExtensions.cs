using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WaffleCLI.Configuration;

/// <summary>
/// Extension methods for configuring application settings and services using Microsoft's dependency injection.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds JSON file-based configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add configuration to.</param>
    /// <param name="configFile">The path to the JSON configuration file. Defaults to "appsettings.json".</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method registers a singleton <see cref="IConfigurationProvider"/> that uses the specified JSON file.
    /// If the file does not exist, the provider will be created with default values.
    /// </remarks>
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services,
        string configFile = "appsettings.json")
    {
        services.AddSingleton<IConfigurationProvider>(_ => new JsonConfigurationProvider(configFile));
        return services;
    }

    /// <summary>
    /// Configures and registers a configuration section as a singleton service in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the configuration section to.</param>
    /// <param name="sectionName">The name of the configuration section to register.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// The configuration section is resolved at runtime using the <see cref="IConfigurationManager"/> service.
    /// The section is registered as a singleton and will be available for dependency injection.
    /// </remarks>
    public static IServiceCollection ConfigureSection(this IServiceCollection services, string sectionName)
    {
        services.AddSingleton(provider =>
        {
            var configManager = provider.GetRequiredService<IConfigurationManager>();
            return configManager.GetSection(sectionName);
        });
        
        return services;
    }
}