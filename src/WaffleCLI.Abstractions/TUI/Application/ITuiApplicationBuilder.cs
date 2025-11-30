using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Abstractions.TUI.Application
{
    /// <summary>
    /// Fluent builder for TUI applications
    /// </summary>
    public interface ITuiApplicationBuilder
    {
        ITuiApplicationBuilder UseRootComponent<T>() where T : class, IComponent;
        ITuiApplicationBuilder ConfigureServices(Action<IServiceCollection> configureAction);
        ITuiApplication Build();
    }
}