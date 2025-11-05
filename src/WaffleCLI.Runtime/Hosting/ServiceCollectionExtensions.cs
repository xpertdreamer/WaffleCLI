using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.Scripting;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Output;
using WaffleCLI.Runtime.Services;
using WaffleCLI.Runtime.Scripting;
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
    /// Registers core services including command executor, console host, output, and script engine.
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
        
        // Runtime services
        services.TryAddTransient<CommandDiscoveryService>();
        
        // Configure default options
        services.Configure<ConsoleHostOptions>(options => { });
        services.Configure<CommandExecutorOptions>(options => { });
        
        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all commands from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assemblies">The assemblies to scan for command types. If empty, uses the entry assembly.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Scans the specified assemblies for classes implementing ICommand and registers them in the command registry.
    /// If no assemblies are specified, uses the entry assembly by default.
    /// </remarks>
    public static IServiceCollection AutoRegisterCommands(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
            assemblies = new[] { Assembly.GetEntryAssembly()! };

        // First register CommandRegistry without initialization
        services.AddSingleton<CommandRegistry>();
        
        // Then register ICommandRegistry with initialization
        services.AddSingleton<ICommandRegistry>(provider =>
        {
            var registry = provider.GetRequiredService<CommandRegistry>();
            registry.Initialize(assemblies);
            return registry;
        });

        return services;
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
}