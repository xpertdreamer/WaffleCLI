using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Rendering;

namespace WaffleCLI.Core.TUI.Elements;

public class ButtonElement : TextElement
{
    public ConsoleColor FocusColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor FocusBackgroundColor { get; set; } = ConsoleColor.Yellow;

    public event Action? Clicked;

    public ButtonElement()
    {
        isFocusable = true;
        Width = 20;
        Height = 3;
        Text = "Button";
        Color = ConsoleColor.White;
        BackgroundColor = ConsoleColor.DarkBlue;
    }

    public override void Render()
    {
        if (!isVisible || _renderEngine == null) return;

        var currentColor = HasFocus ? FocusColor : Color;
        var currentBackground = HasFocus ? FocusBackgroundColor : BackgroundColor;

        // Render button background
        _renderEngine.RenderRect(X, Y, Width, Height, currentBackground, ' ');

        // Render border
        _renderEngine.RenderBorder(X, Y, Width, Height, BorderStyle.Single);

        // Render text
        var textX = X + Math.Max(0, (Width - Text.Length) / 2);
        var textY = Y + Height / 2;
        
        var displayText = GetDisplayText();
        _renderEngine.RenderText(textX, textY, displayText, currentColor, currentBackground);
    }

    public override bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (!HasFocus) return false;

        if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Spacebar)
        {
            Clicked?.Invoke();
            return true;
        }

        return false;
    }

    protected override string GetDisplayText()
    {
        if (string.IsNullOrEmpty(Text)) 
            return new string(' ', Math.Max(1, Width));
        
        // For buttons, we want to center the text, so don't pad right
        var displayText = Text.Length > Width ? Text.Substring(0, Width) : Text;
        return displayText;
    }
}