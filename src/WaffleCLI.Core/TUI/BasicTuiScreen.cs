using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI;

public abstract class BasicTuiScreen : ITuiScreen
{
    private readonly List<ITuiElement> _elements = [];
    public abstract string Title { get; }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task RenderAsync()
    {
        Console.Clear();

        RenderHeader();

        foreach (var element in _elements.Where(e => e.isVisible))
        {
            element.Render();
        }

        RenderFooter();
        
        return Task.CompletedTask;
    }

    public virtual Task HandleInputAsync(ConsoleKeyInfo keyInfo)
    {
        foreach (var element in _elements.Where(e => e.isVisible))
        {
            if (element.HandleInput(keyInfo))
                break;
        }
        
        return Task.CompletedTask;
    }

    protected virtual void RenderHeader()
    {
        var oldFg = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.SetCursorPosition(0, 0);
        Console.Write($"{Title}".PadRight(Console.WindowWidth, '='));
        Console.ForegroundColor = oldFg;
    }

    protected virtual void RenderFooter()
    {
        var oldFg = Console.ForegroundColor;
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(" Press Ctrl+Q to exit ".PadRight(Console.WindowWidth, ' '));
        Console.ForegroundColor = oldFg;
    }

    protected void AddElement(ITuiElement element)
    {
        _elements.Add(element);
    }

    protected void RemoveElement(ITuiElement element)
    {
        _elements.Remove(element);
    }

    protected void ClearElements()
    {
        _elements.Clear();
    }
}