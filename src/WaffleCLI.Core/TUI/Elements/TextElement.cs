using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Elements;

public class TextElement : ITuiElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; } = 1;
    public bool isVisible { get; set; } = true;
    public bool isFocusable { get; set; } = false;

    public string Text {get; set;} = string.Empty;
    public ConsoleColor Color {get; set;} = ConsoleColor.White;
    public ConsoleColor BackgroundColor {get; set;} = ConsoleColor.Black;
    public bool HasBorder {get; set;} = false;
    public ConsoleColor BorderColor {get; set;} = ConsoleColor.Gray;

    public void Render()
    {
        if (!isVisible) return;
        
        var oldFg =  Console.ForegroundColor;
        var oldBg =  Console.BackgroundColor;

        if (HasBorder) RenderBorder();
        
        Console.ForegroundColor = Color;
        Console.BackgroundColor = BackgroundColor;
        
        var renderX = Math.Max(0, Math.Min(X, Console.WindowWidth - 1));
        var renderY = Math.Max(0, Math.Min(Y, Console.WindowHeight - 1));
        
        Console.SetCursorPosition(renderX, renderY);
        
        var displayedText = Text.Length > Width ? Text[..Width] : Text.PadRight(Width);
        Console.Write(displayedText);
        
        Console.ForegroundColor = oldFg;
        Console.BackgroundColor = oldBg;
    }
    
    public bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        return false;
    }

    private void RenderBorder()
    {
        var oldFg = Console.ForegroundColor;
        Console.ForegroundColor = BorderColor;
        
        Console.SetCursorPosition(X - 1, Y - 1);
        Console.Write("+" + new string('-', Width) + "+");

        for (var i = 0; i < Height; i++)
        {
            Console.SetCursorPosition(X - 1, Y + i);
            Console.Write("|");
            Console.SetCursorPosition(X + Width, Y + i);
            Console.Write("|");
        }
        
        Console.SetCursorPosition(X - 1, Y + Height);
        Console.Write("+" + new string('-', Width) + "+");
        
        Console.ForegroundColor = oldFg;
    }
}