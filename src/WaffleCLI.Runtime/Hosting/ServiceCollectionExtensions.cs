using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.Scripting;
using WaffleCLI.Core.Configuration;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Output;
using WaffleCLI.Runtime.Services;
using WaffleCLI.Runtime.Scripting;
using Microsoft.Extensions.Logging;
using WaffleCLI.Core.Middleware;
using WaffleCLI.Runtime.Options;

namespace WaffleCLI.Runtime.Hosting;

/// <summary>
/// Provides extension methods for configuring WaffleCLI services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core WaffleCLI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Registers core services including command executor, console host, output, script engine, and command discovery.
    /// Also configures default options for various WaffleCLI components.
    /// </remarks>
    public static IServiceCollection AddWaffleCli(this IServiceCollection services)
    {
        services.AddOptions();
        
        // Core services
        services.TryAddSingleton<ICommandExecutor, CommandExecutor>();
        services.TryAddSingleton<IConsoleHost, DefaultConsoleHost>();
        services.TryAddSingleton<IConsoleOutput, DefaultConsoleOutput>();
        services.TryAddSingleton<IScriptEngine, ScriptEngine>();
        services.TryAddSingleton<CommandDiscoveryService>();
        
        // Configure default options
        services.Configure<ConsoleHostOptions>(options => { });
        services.Configure<CommandExecutorOptions>(options => { });
        
        return services;
    }

    /// <summary>
    /// Registers a single command type in the dependency injection container.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to register, must implement <see cref="ICommand"/>.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Registers the command as a transient service, ensuring a new instance is created for each execution.
    /// </remarks>
    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services) 
        where TCommand : class, ICommand
    {
        services.AddTransient<TCommand>();
        return services;
    }

    /// <summary>
    /// Registers a command group type in the dependency injection container.
    /// </summary>
    /// <typeparam name="TCommandGroup">The type of command group to register, must implement <see cref="ICommandGroup"/>.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Command groups are registered as transient services and can contain multiple subcommands.
    /// </remarks>
    public static IServiceCollection AddCommandGroup<TCommandGroup>(this IServiceCollection services) 
        where TCommandGroup : class, ICommandGroup
    {
        services.AddTransient<TCommandGroup>();
        return services;
    }

    /// <summary>
    /// Discovers and registers all commands from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assemblies">The assemblies to scan for command types.</param>
    /// <param name="options">The registration options for filtering and configuring command discovery.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Uses <see cref="CommandDiscoveryService"/> to find commands in the specified assemblies and registers them
    /// in both the dependency injection container and the command registry. Applies exclusion filters if specified.
    /// </remarks>
    public static IServiceCollection AddCommandsFromAssemblies(
        this IServiceCollection services, 
        Assembly[] assemblies,
        CommandRegistrationOptions? options = null)
    {
        options ??= new CommandRegistrationOptions();
        
        // Create temporary ServiceProvider for command discovery
        var serviceProvider = services.BuildServiceProvider();
        var discoveryService = new CommandDiscoveryService(
            serviceProvider.GetRequiredService<ILogger<CommandDiscoveryService>>());
        
        var result = discoveryService.DiscoverCommands(assemblies, options);

        // Register commands in DI
        foreach (var commandType in result.AllCommandTypes)
        {
            if (ShouldRegisterCommand(commandType, options))
            {
                services.AddTransient(commandType);
            }
        }

        // Register CommandRegistry
        services.TryAddSingleton<ICommandRegistry>(provider =>
        {
            var registry = new CommandRegistry(
                provider,
                provider.GetRequiredService<ILogger<CommandRegistry>>());
            
            registry.Initialize(result);
            return registry;
        });

        return services;
    }

    /// <summary>
    /// Determines whether a command type should be registered based on the registration options.
    /// </summary>
    /// <param name="commandType">The command type to check.</param>
    /// <param name="options">The registration options containing exclusion filters.</param>
    /// <returns>True if the command should be registered; otherwise, false.</returns>
    /// <remarks>
    /// Checks the command name against the excluded commands list in the registration options.
    /// Command names are derived by removing "Command" suffix and converting to lowercase.
    /// </remarks>
    private static bool ShouldRegisterCommand(Type commandType, CommandRegistrationOptions options)
    {
        var commandName = commandType.Name.Replace("Command", "").ToLowerInvariant();
        
        // Check exclusions
        if (options.ExcludedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }

    /// <summary>
    /// Registers a command middleware in the dependency injection container.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of middleware to register, must implement <see cref="ICommandMiddleware"/>.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Middleware are executed in the order they are registered and can intercept command execution for cross-cutting concerns.
    /// </remarks>
    public static IServiceCollection AddCommandMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, ICommandMiddleware
    {
        services.AddTransient<ICommandMiddleware, TMiddleware>();
        return services;
    }

    /// <summary>
    /// Registers the default set of command middleware for common functionality.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Includes exception handling, execution timing, and validation middleware to provide
    /// comprehensive command execution pipeline with error handling and monitoring.
    /// </remarks>
    public static IServiceCollection AddDefaultMiddleware(this IServiceCollection services)
    {
        services.AddCommandMiddleware<ExceptionHandlingMiddleware>();
        services.AddCommandMiddleware<TimingMiddleware>();
        services.AddCommandMiddleware<ValidationMiddleware>();
        return services;
    }
}