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
        ITuiApplicationBuilder UseRootComponent(Func<IServiceProvider, IComponent> factory);
        ITuiApplicationBuilder ConfigureServices(Action<IServiceCollection> configureAction);
        ITuiApplicationBuilder WithFrameRate(int frameRate);
        ITuiApplicationBuilder WithTheme(string themeName);
        ITuiApplicationBuilder EnableDoubleBuffering(bool enable = true);
        ITuiApplicationBuilder EnableInputLogging(bool enable = true);
        ITuiApplication Build();
    }
    
    /// <summary>
    /// Extended application configuration
    /// </summary>
    public interface ITuiApplicationConfiguration
    {
        int FrameRate { get; set; }
        string Theme { get; set; }
        bool EnableDoubleBuffering { get; set; }
        bool EnableInputLogging { get; set; }
        int InitialWidth { get; set; }
        int InitialHeight { get; set; }
    }
}