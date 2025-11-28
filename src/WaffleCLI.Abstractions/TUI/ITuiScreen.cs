namespace WaffleCLI.Abstractions.TUI;

public interface ITuiScreen
{
    string Title { get; }
    Task InitializeAsync();
    Task RenderAsync();
    Task HandleInputAsync(ConsoleKeyInfo keyInfo);
    Task HandleResizeAsync();
}