using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Elements;

public class ButtonElement : ITuiElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 3;
    public bool isVisible { get; set; } = true;
    public bool isFocusable { get; set; } = true;
    public bool HasFocus {get; set;}

    public string Text { get; set; } = "Button";
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
    public ConsoleColor BackgroundColor {get; set;} = ConsoleColor.DarkBlue;
    public ConsoleColor FocusColor {get; set;} = ConsoleColor.Black;
    public ConsoleColor FocusBackgroundColor {get; set;} = ConsoleColor.Yellow;
    
    public event Action? Clicked;

    public void Render()
    {
        if (!isVisible) return;
        
        var oldFg =  Console.ForegroundColor;
        var oldBg =  Console.BackgroundColor;

        if (HasFocus)
        {
            Console.ForegroundColor = FocusColor;
            Console.BackgroundColor = FocusBackgroundColor;
        }
        else
        {
            Console.ForegroundColor = Color;
            Console.BackgroundColor = BackgroundColor;
        }

        for (var row = 0; row < Height; row++)
        {
            Console.SetCursorPosition(X, Y + row);
            if (row == 0 || row == Height - 1)
                Console.Write("+" + new string('-', Width - 2) + "+");
            else
            {
                Console.Write("|");

                if (row == Height / 2)
                {
                    var paddedText = Text.PadBoth(Width - 2);
                    Console.Write(paddedText);
                }
                else
                {
                    Console.Write(new string(' ', Width - 2));
                }
                
                Console.Write("|");
            }
        }

        Console.ForegroundColor = oldFg;
        Console.BackgroundColor = oldBg;
    }

    public bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (!HasFocus) return false;

        if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Spacebar) return false;
        Clicked?.Invoke();
        
        return true;
    }
}

public static class StringExtensions
{
    public static string PadBoth(this string str, int length)
    {
        var spaces = length - str.Length;
        var padLeft = spaces / 2 + str.Length;
        return str.PadLeft(padLeft).PadRight(length);
    }
}