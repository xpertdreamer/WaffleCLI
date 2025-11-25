using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Elements;

public class TextElement : ITuiElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool isVisible { get; set; }
    
    public string Text {get; set;} = string.Empty;
    public ConsoleColor Color {get; set;} = ConsoleColor.White;
    public ConsoleColor BackgroundColor {get; set;} = ConsoleColor.Black;

    public void Render()
    {
        if (!isVisible) return;
        
        var oldFg =  Console.ForegroundColor;
        var oldBg =  Console.BackgroundColor;
        
        Console.ForegroundColor = Color;
        Console.BackgroundColor = BackgroundColor;
        
        Console.SetCursorPosition(X, Y);
        
        var displayedText = Text.Length > Width ? Text[..Width] : Text;
        Console.Write(displayedText);
        
        Console.ForegroundColor = oldFg;
        Console.BackgroundColor = oldBg;
    }

    public bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        return false;
    }
}