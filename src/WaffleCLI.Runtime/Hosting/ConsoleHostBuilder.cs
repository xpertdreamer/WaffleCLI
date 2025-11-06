using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Core.Configuration;
using WaffleCLI.Runtime.Options;

namespace WaffleCLI.Runtime.Hosting;

/// <summary>
/// Provides a fluent interface for building and configuring the console application host.
/// </summary>
/// <remarks>
/// This builder wraps the standard .NET Generic Host and adds WaffleCLI-specific configurations
/// and service registrations for building command-line applications with extensive customization options.
/// </remarks>
public class ConsoleHostBuilder
{
    private readonly IHostBuilder _hostBuilder;
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly List<Action<HostBuilderContext, IServiceCollection>> _contextServiceConfigurations = new();
    
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
    /// Configures the application to use JSON configuration files and environment variables.
    /// </summary>
    /// <param name="configFile">The name of the JSON configuration file. Defaults to "appsettings.json".</param>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// Adds a JSON configuration file and environment variables with the "WaffleCLI_" prefix.
    /// The configuration file is optional and will be reloaded if changed.
    /// </remarks>
    public ConsoleHostBuilder UseConfiguration(string configFile = "appsettings.json")
    {
        _hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(configFile, optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables("WaffleCLI_");
        });
        return this;
    }

    /// <summary>
    /// Adds a service configuration delegate to be applied during host building.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="IServiceCollection"/>.</param>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// Service configurations are applied in the order they are added, after the core WaffleCLI services.
    /// </remarks>
    public ConsoleHostBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Configures services with access to the host builder context.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="IServiceCollection"/> with context.</param>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    public ConsoleHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
    {
        _hostBuilder.ConfigureServices(configure);
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
    /// Configures default logging with console output and Information minimum level.
    /// </summary>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// Adds console logging provider and sets the minimum log level to Information.
    /// </remarks>
    public ConsoleHostBuilder UseDefaultLogging()
    {
        _hostBuilder.ConfigureLogging((context,logging) =>
        {
            logging.AddConsole();

            var cliOptions = new CliOptions();
            context.Configuration.Bind(cliOptions);

            logging.SetMinimumLevel(Enum.TryParse<LogLevel>(cliOptions.Logging?.MinimumLevel, out var minimumLevel)
                ? minimumLevel
                : LogLevel.Information);
        });
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
    /// Configures services based on the <see cref="CliOptions"/> from application configuration.
    /// </summary>
    /// <returns>The current <see cref="ConsoleHostBuilder"/> instance for chaining.</returns>
    /// <remarks>
    /// Binds configuration to <see cref="CliOptions"/> and applies configuration-based service settings
    /// such as logging levels and automatic command registration.
    /// </remarks>
    public ConsoleHostBuilder ConfigureFromOptions()
    {
        _hostBuilder.ConfigureServices((context, services) =>
        {
            // Register configuration
            services.Configure<CliOptions>(context.Configuration);
            
            // Configure services based on configuration
            var cliOptions = new CliOptions();
            context.Configuration.Bind(cliOptions);
            
            ConfigureServicesFromOptions(services, cliOptions);
        });
        
        return this;
    }
    
    /// <summary>
    /// Configures services based on the provided <see cref="CliOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">The CLI options to use for configuration.</param>
    /// <remarks>
    /// Applies logging configuration and automatic command registration based on the provided options.
    /// </remarks>
    private void ConfigureServicesFromOptions(IServiceCollection services, CliOptions options)
    {
        // Configure logging
        if (!options.Logging.EnableLogging)
        {
            services.Configure<LoggerFilterOptions>(opt => 
                opt.MinLevel = LogLevel.None);
        }

        // Automatic command registration if enabled
        if (options.CommandRegistration.AutoDiscoverCommands)
        {
            var assemblies = GetAssembliesToScan(options.CommandRegistration.AssembliesToScan);
            services.AddCommandsFromAssemblies(assemblies, options.CommandRegistration);
        }
    }
    
    /// <summary>
    /// Gets the assemblies to scan for command discovery based on configuration.
    /// </summary>
    /// <param name="assemblyNames">The names of assemblies to scan.</param>
    /// <returns>An array of assemblies to scan for commands.</returns>
    /// <remarks>
    /// If no assembly names are specified, returns the entry assembly.
    /// Handles assembly loading errors gracefully with warning messages.
    /// </remarks>
    private Assembly[] GetAssembliesToScan(string[] assemblyNames)
    {
        if (assemblyNames.Length == 0)
            return new[] { Assembly.GetEntryAssembly()! };

        var assemblies = new List<Assembly>();
        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not load assembly {assemblyName}: {ex.Message}");
            }
        }
        
        return assemblies.ToArray();
    }
    
    /// <summary>
    /// Builds the configured host and returns the console host instance.
    /// </summary>
    /// <returns>An <see cref="IConsoleHost"/> instance ready for execution.</returns>
    /// <exception cref="Exception">Thrown when host building fails, with detailed error information.</exception>
    /// <remarks>
    /// Applies all accumulated service configurations and builds the host with WaffleCLI core services.
    /// </remarks>
    public IConsoleHost Build()
    {
        // Apply all service configurations
        _hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddWaffleCli();
            
            services.Configure<CliOptions>(context.Configuration);
            
            var cliOptions = new CliOptions();
            context.Configuration.Bind(cliOptions);
            
            ConfigureFromCliOptions(services, cliOptions);
            
            foreach (var config in _serviceConfigurations)
            {
                config(services);
            }
            
            foreach (var config in _contextServiceConfigurations)
            {
                config(context, services);
            }

            if (cliOptions.CommandRegistration?.AutoDiscoverCommands != true) return;
            var assemblies = GetAssembliesToScan(cliOptions.CommandRegistration.AssembliesToScan);
            services.AddCommandsFromAssemblies(assemblies, cliOptions.CommandRegistration);
        });

        try
        {
            var host = _hostBuilder.Build();
            return host.Services.GetRequiredService<IConsoleHost>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Build failed: {ex.Message}");
            throw;
        }
    }

    private void ConfigureFromCliOptions(IServiceCollection services, CliOptions cliOptions)
    {
        services.Configure<ConsoleHostOptions>(options =>
            {
                options.ShowWelcomeMessage = cliOptions.Host?.ShowWelcomeMessage ?? true;
                options.ExitOnNonZeroExitCode = cliOptions.Host?.ExitOnNonZeroExitCode ?? false;
            });

        services.Configure<CommandExecutorOptions>(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(cliOptions.Scripting?.MaxExecutionTimeSeconds ?? 300);
            options.AllowParallelExecution = false;
        });
    }
}