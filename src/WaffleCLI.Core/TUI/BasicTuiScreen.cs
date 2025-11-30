using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Elements;
using WaffleCLI.Core.TUI.Rendering;

namespace WaffleCLI.Core.TUI;

public abstract class BasicTuiScreen : ITuiScreen
{
    protected readonly List<ITuiElement> _elements = [];
    protected int _focusedElementIndex = -1;
    protected bool _firstRender = true;
    private (int width, int height) _lastSize;
    protected bool _needsLayoutRecalculation = true;
    
    private TextElement _headerElement;
    private TextElement _footerElement;
    private bool _isInitialized = false;
    
    protected RenderLayerManager LayerManager => 
        ServiceLocator.GetService<RenderLayerManager>();
    
    protected ConfigurationManager ConfigManager => 
        ServiceLocator.GetService<ConfigurationManager>();
    
    protected IRenderEngine RenderEngine =>
        ServiceLocator.GetService<IRenderEngine>();
    
    public abstract string Title { get; }

    public virtual async Task InitializeAsync()
    {
        _needsLayoutRecalculation = true;

        await RegisterElementsInLayers();
        _isInitialized = true;
    }
    
    protected virtual async Task RegisterElementsInLayers()
    {
        await CreateHeaderAndFooter();
        
        foreach (var element in _elements)
        {
            LayerManager.AddElementsToLayer("content", element);
        }
    }
    
    protected virtual async Task CreateHeaderAndFooter()
    {
        var config = ConfigManager.Config;
        var theme = config.Theme.Themes[config.Theme.Current];
        
        _headerElement = new TextElement
        {
            X = 0, 
            Y = 0,
            Width = RenderEngine.Width,
            Height = 1,
            Text = $" {Title} ",
            Color = ParseColor(theme.Colors.Text),
            BackgroundColor = ParseColor(theme.Colors.Primary),
            isFocusable = false,
            isVisible = true
        };
        
        LayerManager.AddElementsToLayer("header", _headerElement);

        _footerElement = new TextElement
        {
            X = 0, 
            Y = RenderEngine.Height - 1,
            Width = RenderEngine.Width,
            Height = 1,
            Text = " Tab:Navigate | Enter:Select | Ctrl+Q:Exit ",
            Color = ParseColor(theme.Colors.Text),
            BackgroundColor = ParseColor(theme.Colors.Secondary),
            isFocusable = false,
            isVisible = true
        };
        
        LayerManager.AddElementsToLayer("footer", _footerElement);

        await Task.CompletedTask;
    }

    protected virtual void UpdateHeaderAndFooter()
    {
        _headerElement.Width = RenderEngine.Width;
        _headerElement.Text = $" {Title} ";

        _footerElement.Width = RenderEngine.Width;
        _footerElement.Y = RenderEngine.Height - 1;
    }

    public virtual async Task HandleResizeAsync()
    {
        if (!_isInitialized) return;
        
        _needsLayoutRecalculation = true;
        _firstRender = true;
        
        UpdateHeaderAndFooter();
        
        await Task.CompletedTask;
    }

    public virtual async Task RenderAsync()
    {
        if (!_isInitialized) return;
        
        if (_firstRender || Console.WindowWidth != _lastSize.width || Console.WindowHeight != _lastSize.height)
        {
            _firstRender = false;
            _lastSize = (Console.WindowWidth, Console.WindowHeight);
            _needsLayoutRecalculation =  true;
        }

        if (!_needsLayoutRecalculation) return;
        RecalculateLayout();
        _needsLayoutRecalculation = false;

        await Task.CompletedTask;
    }

    protected virtual void RecalculateLayout()
    {
        var screenWidth = Console.WindowWidth;
        var screenHeight = Console.WindowHeight;

        foreach (var element in _elements)
        {
            if(element.X + element.Width > screenWidth)
                element.X = Math.Max(0, (screenWidth - element.Width) / 2);
            if (element.Y + element.Height > screenHeight - 2)
                element.Y = Math.Max(1, (screenHeight - element.Height - 2) / 2);
        }
    }

    private static ConsoleColor ParseColor(string colorName)
    {
        return Enum.TryParse<ConsoleColor>(colorName, true, out var color) 
            ? color 
            : ConsoleColor.White;
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
        Console.ResetColor();
        
        for (var row = 1; row < Console.WindowHeight - 1; row++)
        {
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', Console.WindowWidth));
        }
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
        
        Console.ResetColor();
    }

    protected virtual void RenderFooter()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.BackgroundColor = ConsoleColor.Black;
        
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        const string footerText = " Tab:Navigate | Enter:Select | Ctrl+Q:Exit";
        Console.Write(footerText);
        
        Console.ResetColor();
    }

    protected void AddElement(ITuiElement element)
    {
        _elements.Add(element);

        if (_focusedElementIndex != -1 || !element.isVisible || !element.isFocusable) return;
        _focusedElementIndex = _elements.Count - 1;
        UpdateFocus();
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

    protected void MoveFocusNext()
    {
        if (_elements.Count == 0) return;

        var focusableElements = _elements
            .Where(e => e is { isVisible: true, isFocusable: true })
            .ToList();
        
        if (focusableElements.Count == 0) return;

        var currentIndex = GetVisibleElementIndex(_focusedElementIndex);
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