using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Hosting;

namespace WaffleCLI.Runtime.Hosting;

/// <summary>
/// Provides a fluent interface for building and configuring the console application host.
/// </summary>
/// <remarks>
/// This builder wraps the standard .NET Generic Host and adds WaffleCLI-specific configurations
/// and service registrations for building command-line applications.
/// </remarks>
public class ConsoleHostBuilder
{
    private readonly IHostBuilder _hostBuilder;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleHostBuilder"/> class with default configuration.
    /// </summary>
    public ConsoleHostBuilder() : this([])
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleHostBuilder"/> class with specified command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments to pass to the host configuration.</param>
    private ConsoleHostBuilder(string[] args)
    {
        _hostBuilder = Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
    }

    /// <summary>
    /// Configures the application configuration using the specified delegate.
    /// </summary>
    /// <param name="configureDelegate">The delegate to configure the <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    public ConsoleHostBuilder ConfigureAppConfiguration(
        Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _hostBuilder.ConfigureAppConfiguration(configureDelegate);
        return this;
    }

    /// <summary>
    /// Configures the services for the application, including automatic WaffleCLI service registration.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="IServiceCollection"/>.</param>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// Automatically calls <see cref="WaffleCliServiceCollectionExtensions.AddWaffleCli"/> before invoking the custom configure delegate.
    /// </remarks>
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
    /// Configures the logging system for the application.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="ILoggingBuilder"/>.</param>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    public ConsoleHostBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
    {
        _hostBuilder.ConfigureLogging(configure);
        return this;
    }
    
    /// <summary>
    /// Configures the host to use the console lifetime, which listens for Ctrl+C or SIGTERM to shutdown.
    /// </summary>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    public ConsoleHostBuilder UseConsoleLifetime()
    {
        _hostBuilder.UseConsoleLifetime();
        return this;
    }
    
    /// <summary>
    /// Builds the configured host and returns the console host instance.
    /// </summary>
    /// <returns>An <see cref="IConsoleHost"/> instance ready for execution.</returns>
    /// <exception cref="Exception">Thrown when host building fails, with detailed error information written to console.</exception>
    /// <remarks>
    /// Provides detailed error information including inner exceptions when host construction fails.
    /// </remarks>
    public IConsoleHost Build()
    {
        try
        {
            var host = _hostBuilder.Build();
            return host.Services.GetRequiredService<IConsoleHost>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Build failed: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            throw;
        }
    }
}