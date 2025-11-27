using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Screens;

namespace WaffleCLI.Core.TUI;

public class TuiApplicationBuilder
{
    private readonly IServiceCollection _services;
    private Type _startScreenType = typeof(WelcomeScreen);

    public TuiApplicationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public TuiApplicationBuilder UseStartScreen<T>() where T : class, ITuiScreen
    {
        _startScreenType = typeof(T);
        _services.AddTransient<T>();
        return this;
    }
    
    public TuiApplicationBuilder AddScreen<T>() where T : class, ITuiScreen
    {
        _services.AddTransient<T>();
        return this;
    }

    public TuiApplicationBuilder AddElement<T>() where T : class, ITuiElement
    {
        _services.AddTransient<T>();
        return this;
    }

    public TuiApplicationBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(_services);
        return this; 
    }

    public Type GetStartScreenType() =>  _startScreenType;
}