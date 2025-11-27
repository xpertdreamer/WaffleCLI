using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI;

public abstract class BasicTuiScreen : ITuiScreen
{
    protected readonly List<ITuiElement> _elements = [];
    private int _focusedElementIndex = -1;
    private bool _firstRender = true;
    private string _lastRenderedTitle = string.Empty;
    public abstract string Title { get; }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task RenderAsync()
    {
        if (_firstRender)
        {
            Console.Clear();
            _firstRender = false;
            _lastRenderedTitle = string.Empty;
        }

        if (_lastRenderedTitle != Title)
        {
            RenderHeader();
            _lastRenderedTitle = Title;
        }

        ClearContentArea();

        foreach (var element in _elements.Where(e => e.isVisible))
        {
            element.Render();
        }

        RenderFooter();
        
        return Task.CompletedTask;
    }

    public virtual Task HandleInputAsync(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == ConsoleKey.Tab)
        {
            MoveFocusNext();
            return Task.CompletedTask;
        }

        if (keyInfo is { Key: ConsoleKey.Q, Modifiers: ConsoleModifiers.Control })
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }

        if (_focusedElementIndex >= 0 && _focusedElementIndex < _elements.Count)
        {
            var focusedElement = _elements[_focusedElementIndex];
            if (focusedElement.HandleInput(keyInfo))
            {
                return Task.CompletedTask;
            }
        }
        
        return Task.CompletedTask;
    }
    
    protected virtual void ClearContentArea()
    {
        var originalLeft = Console.CursorLeft;
        var originalTop = Console.CursorTop;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;

        // Очищаем область между заголовком (строка 1) и футером (последняя строка)
        for (int row = 1; row < Console.WindowHeight - 1; row++)
        {
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth));
        }

        Console.SetCursorPosition(originalLeft, originalTop);
    }

    protected virtual void RenderHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.BackgroundColor = ConsoleColor.Black;
        
        Console.SetCursorPosition(0, 0);
        Console.Write(new string(' ', Console.WindowWidth));
        
        Console.SetCursorPosition(0, 0);
        var titleText = $" {Title} ";
        Console.Write(titleText);
        
        var remainWidth = Console.WindowWidth - titleText.Length;
        if (remainWidth > 0)
        {
            Console.Write(new string('=', remainWidth));
        }
    }

    protected virtual void RenderFooter()
    {
        
        var originalLeft = Console.CursorLeft;
        var originalTop = Console.CursorTop;
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.BackgroundColor = ConsoleColor.Black;
        
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        const string footerText = " Tab:Navigate | Enter:Select | Ctrl+Q:Exit";
        Console.Write(footerText);
        
        Console.ResetColor();
        Console.SetCursorPosition(originalLeft, originalTop);
    }

    protected void AddElement(ITuiElement element)
    {
        _elements.Add(element);

        if (_focusedElementIndex == -1 && element.isVisible)
        {
            _focusedElementIndex = _elements.Count - 1;
            UpdateFocus();
        }
    }

    protected void RemoveElement(ITuiElement element)
    {
        var index = _elements.IndexOf(element);
        _elements.Remove(element);

        if (index == _focusedElementIndex)
        {
            MoveFocusNext();
        }
    }

    protected void ClearElements()
    {
        _elements.Clear();
        _focusedElementIndex = -1;
    }

    protected void SetFocus(ITuiElement element)
    {
        var index = _elements.IndexOf(element);
        if (index < 0) return;
        _focusedElementIndex = index;
        UpdateFocus();
    }

    private void MoveFocusNext()
    {
        if (_elements.Count == 0) return;

        var focusableElements = _elements
            .Where(e => e.isVisible && e.isFocusable)
            .ToList();
        
        if (focusableElements.Count == 0) return;

        var currentIndex = _focusedElementIndex >= 0
            ? _elements.IndexOf(focusableElements.FirstOrDefault(e => _elements.IndexOf(e) == _focusedElementIndex) ??
                                focusableElements[0])
            : -1;

        var nextIndex = (currentIndex + 1) % focusableElements.Count;
        _focusedElementIndex = _elements.IndexOf(focusableElements[nextIndex]);
        
        UpdateFocus();
    }

    private void UpdateFocus()
    {
        for (var i = 0; i < _elements.Count; i++)
        {
            var element = _elements[i];
            var hasFocusProperty =  element.GetType().GetProperty("HasFocus");
            if (hasFocusProperty != null && hasFocusProperty.CanWrite)
            {
                hasFocusProperty.SetValue(element, i == _focusedElementIndex);
            }
        }
    }

    private int GetVisibleElementIndex(int elementIndex)
    {
        if (elementIndex < 0 || elementIndex >= _elements.Count)
            return -1;

        var visibleElements = _elements.Where(e => e.isVisible).ToList();
        var element = visibleElements[elementIndex];
        return visibleElements.IndexOf(element);
    }
}