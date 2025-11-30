using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Rendering;

namespace WaffleCLI.Core.TUI.Elements;

public class TextElement : ITuiElement, IRenderEngineAware
{
    protected IRenderEngine _renderEngine;

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; } = 1;
    public bool isVisible { get; set; } = true;
    public bool isFocusable { get; set; } = false;
    public bool HasFocus { get; set; }

    public string Text { get; set; } = string.Empty;
    public ConsoleColor Color { get; set; } = ConsoleColor.White;
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public bool HasBorder { get; set; } = false;
    public ConsoleColor BorderColor { get; set; } = ConsoleColor.Gray;

    public void SetRenderEngine(IRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    public virtual void Render()
    {
        if (!isVisible || _renderEngine == null) return;

        if (HasBorder)
        {
            RenderBorder();
        }

        var displayText = GetDisplayText();
        _renderEngine.RenderText(X, Y, displayText, Color, BackgroundColor);
    }

    public virtual bool HandleInput(ConsoleKeyInfo keyInfo) => false;

    protected virtual void RenderBorder()
    {
        if (_renderEngine == null) return;

        _renderEngine.RenderBorder(X - 1, Y - 1, Width + 2, Height + 2, BorderStyle.Single);
    }

    protected virtual string GetDisplayText()
    {
        if (string.IsNullOrEmpty(Text)) 
            return new string(' ', Math.Max(1, Width));
        
        var displayText = Text.Length > Width ? Text.Substring(0, Width) : Text;
        return displayText.PadRight(Width);
    }
}