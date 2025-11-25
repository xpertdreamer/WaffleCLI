namespace WaffleCLI.Abstractions.TUI;

public interface ITuiApplication
{
    Task RunAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}