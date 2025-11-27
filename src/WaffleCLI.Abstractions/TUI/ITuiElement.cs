namespace WaffleCLI.Abstractions.TUI;

public interface ITuiElement
{
    int X {get; set;}
    int Y {get; set;}
    int Width {get; set;}
    int Height {get; set;}
    bool isVisible {get; set;}
    bool isFocusable { get; set; }

    void Render();
    bool HandleInput(ConsoleKeyInfo keyInfo);
}