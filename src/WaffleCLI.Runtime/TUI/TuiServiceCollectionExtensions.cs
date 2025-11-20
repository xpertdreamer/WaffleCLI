using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Output;
using WaffleCLI.Runtime.Services;
using WaffleCLI.Runtime.TUI.Screens;

namespace WaffleCLI.Runtime.TUI;

/// <summary>
/// Provides extension methods for registering TUI (Text User Interface) services in the dependency injection container.
/// </summary>
public static class TuiServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures WaffleCLI TUI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection for chaining.</returns>
    /// <remarks>
    /// Registers the console-based TUI application, main screen, and replaces the standard command executor
    /// with a TUI-specific implementation that provides enhanced integration with the text user interface.
    /// </remarks>
    public static IServiceCollection AddWaffleTui(this IServiceCollection services)
    {
        services.TryAddSingleton<ITuiApplication, ConsoleTuiApplication>();
        
        services.TryAddSingleton<MainScreen>();

        services.RemoveAll<ICommandExecutor>();
        services.TryAddSingleton<ICommandExecutor, TuiCommandExecutor>();
        
        services.TryAddSingleton<IConsoleOutput, DefaultConsoleOutput>();

        return services;
    }
}