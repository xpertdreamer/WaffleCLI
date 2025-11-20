using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Hosting;
using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Runtime.Tui;

public class TuiApplicationHost : IApplicationHost
{
    private readonly ITuiApplication _tuiApplication;
    private readonly ILogger<TuiApplicationHost> _logger;

    public TuiApplicationHost(
        ITuiApplication tuiApplication,
        ILogger<TuiApplicationHost> logger)
    {
        _tuiApplication = tuiApplication;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting TUI application host");
            await _tuiApplication.RunAsync(cancellationToken);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TUI application host failed");
            return 1;
        }
    }
}