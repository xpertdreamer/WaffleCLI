using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Rendering;

namespace WaffleCLI.Core.TUI.Elements;

public class ButtonElement : ITuiElement, IRenderEngineAware
{
    private IRenderEngine? _renderEngine;

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 3;
    public bool isVisible { get; set; } = true;
    public bool isFocusable { get; set; } = true;
    public bool HasFocus { get; set; }

    public string Text { get; set; } = "Button";
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.DarkBlue;
    public ConsoleColor FocusColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Yellow;

    public event Action? Clicked;

    public void SetRenderEngine(IRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    public void Render()
    {
        if (!isVisible || _renderEngine == null) return;

        var currentColor = HasFocus ? FocusColor : Color;
        var currentBackground = HasFocus ? FocusBackgroundColor : BackgroundColor;

        // Render button background
        _renderEngine.RenderRect(X, Y, Width, Height, currentBackground, ' ');

        // Render border
        _renderEngine.RenderBorder(X, Y, Width, Height, BorderStyle.Single);

        // Render text
        var textX = X + (Width - Text.Length) / 2;
        var textY = Y + Height / 2;
        _renderEngine.RenderText(textX, textY, Text, currentColor, currentBackground);
    }

    public bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (!HasFocus) return false;

        if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Spacebar)
        {
            Clicked?.Invoke();
            return true;
        }

        return false;
    }
}