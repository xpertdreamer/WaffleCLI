namespace WaffleCLI.Abstractions.TUI;

public interface ITuiApplication
{
    Task RunAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}

public interface ITuiScreen
{
    string Title { get; }
    Task InitializeAsync();
    Task RenderAsync();
    Task HandleKeyAsync(ConsoleKeyInfo keyInfo);
}