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
    public ConsoleHostBuilder() : this([])
    {
    }
    
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