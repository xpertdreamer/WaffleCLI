using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Hosting.Extensions;

namespace WaffleCLI.Hosting;

/// <summary>
/// A builder for creating console application hosts
/// </summary>
public class ConsoleHostBuilder
{
    private readonly IHostBuilder _hostBuilder;
    
    /// <summary>
    /// Initializes a new instance of the ConsoleHostBuilder class
    /// </summary>
    /// <param name="args">The command line arguments</param>
    public ConsoleHostBuilder(string[] args)
    {
        _hostBuilder = Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
    }

    /// <summary>
    /// Configures the application configuration
    /// </summary>
    /// <param name="configureDelegate">The configuration delegate</param>
    /// <returns>The console host builder</returns>
    public ConsoleHostBuilder ConfigureAppConfiguration(
        Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _hostBuilder.ConfigureAppConfiguration(configureDelegate);
        return this;
    }

    /// <summary>
    /// Configures the services
    /// </summary>
    /// <param name="configure">The configuration delegate</param>
    /// <returns>The console host builder</returns>
    public ConsoleHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
    {
        _hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddWaffleCli();
            configure?.Invoke(context, services);
        });
        return this;
    } 
    
    /// <summary>
    /// Configures the logging
    /// </summary>
    /// <param name="configure">The configuration delegate</param>
    /// <returns>The console host builder</returns>
    public ConsoleHostBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
    {
        _hostBuilder.ConfigureLogging(configure);
        return this;
    }
    
    
    /// <summary>
    /// Uses the console lifetime
    /// </summary>
    /// <returns>The console host builder</returns>
    public ConsoleHostBuilder UseConsoleLifetime()
    {
        _hostBuilder.UseConsoleLifetime();
        return this;
    }
    
    /// <summary>
    /// Builds the console host
    /// </summary>
    /// <returns>The console host</returns>
    public IConsoleHost Build()
    {
        var host = _hostBuilder.Build();
        return host.Services.GetRequiredService<IConsoleHost>();
    }
}

/// <summary>
/// Defines a contract for application startup configuration in a console application.
/// Implement this interface to configure services and application components during startup.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IConsoleStartup"/> interface is typically implemented by classes that
/// perform application initialization tasks such as:
/// </para>
/// <list type="bullet">
/// <item><description>Registering commands with the command registry</description></item>
/// <item><description>Configuring application-specific services</description></item>
/// <item><description>Setting up default configuration values</description></item>
/// <item><description>Performing runtime validation of services</description></item>
/// <item><description>Initializing application state</description></item>
/// </list>
/// <para>
/// Implementations are discovered and executed automatically by the console host during application startup.
/// </para>
/// </remarks>
public interface IConsoleStartup
{
    /// <summary>
    /// Configures the application services and components after the service provider has been built.
    /// </summary>
    /// <param name="services">The service provider that can be used to resolve required services.</param>
    /// <remarks>
    /// <para>
    /// This method is called after the dependency injection container has been fully constructed
    /// but before the interactive console loop begins. Use this method to perform initialization
    /// that requires access to resolved services.
    /// </para>
    /// <para>
    /// Common tasks performed in this method include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Resolving <see cref="ICommandRegistry"/> and registering application commands</description></item>
    /// <item><description>Configuring default settings or options</description></item>
    /// <item><description>Validating that required services are properly configured</description></item>
    /// <item><description>Setting up initial application state</description></item>
    /// <item><description>Running database migrations or other setup tasks</description></item>
    /// </list>
    /// </remarks>
    void Configure(IServiceProvider services);
}