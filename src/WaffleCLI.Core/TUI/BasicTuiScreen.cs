using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Diagnostics;
using WaffleCLI.Core.TUI.Elements;
using WaffleCLI.Core.TUI.Rendering;

namespace WaffleCLI.Core.TUI;

public abstract class BasicTuiScreen : ITuiScreen
{
    protected readonly List<ITuiElement> _elements = new();
    protected int _focusedElementIndex = -1;
    protected bool _firstRender = true;
    private (int width, int height) _lastSize;
    protected bool _needsLayoutRecalculation = true;
    
    private TextElement _headerElement;
    private TextElement _footerElement;
    private bool _isInitialized = false;
    
    // Focus management optimization
    private List<ITuiElement> _focusableElements = new();
    private bool _focusCacheInvalid = true;
    
    protected RenderLayerManager LayerManager => ServiceLocator.GetService<RenderLayerManager>();
    protected ConfigurationManager ConfigManager => ServiceLocator.GetService<ConfigurationManager>();
    protected IRenderEngine RenderEngine => ServiceLocator.GetService<IRenderEngine>();
    
    public abstract string Title { get; }

    public virtual async Task InitializeAsync()
    {
        _needsLayoutRecalculation = true;
        await RegisterElementsInLayers();
        RebuildFocusCache();
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
        InvalidateFocusCache();
        
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
            _needsLayoutRecalculation = true;
        }

        if (_needsLayoutRecalculation)
        {
            RecalculateLayout();
            _needsLayoutRecalculation = false;
        }

        await Task.CompletedTask;
    }

    protected virtual void RecalculateLayout()
    {
        var screenWidth = Console.WindowWidth;
        var screenHeight = Console.WindowHeight;

        foreach (var element in _elements)
        {
            if (element.X + element.Width > screenWidth)
                element.X = Math.Max(0, (screenWidth - element.Width) / 2);
            if (element.Y + element.Height > screenHeight - 2)
                element.Y = Math.Max(1, (screenHeight - element.Height - 2) / 2);
        }
        
        InvalidateFocusCache();
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
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                MoveFocusPrevious();
            else
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

    // Focus management methods
    private void InvalidateFocusCache() => _focusCacheInvalid = true;

    private void RebuildFocusCache()
    {
        if (!_focusCacheInvalid) return;
        
        _focusableElements = _elements
            .Where(e => e is { isVisible: true, isFocusable: true })
            .ToList();
        _focusCacheInvalid = false;

        // Validate current focus index
        if (_focusedElementIndex >= 0 && _focusedElementIndex < _elements.Count)
        {
            var focusedElement = _elements[_focusedElementIndex];
            if (!focusedElement.isVisible || !focusedElement.isFocusable)
            {
                MoveFocusNext();
            }
        }
        else if (_focusableElements.Count > 0)
        {
            // Auto-focus first focusable element
            _focusedElementIndex = _elements.IndexOf(_focusableElements[0]);
            UpdateFocus();
        }
    }

    protected void AddElement(ITuiElement element)
    {
        if (element == null) return;
        
        _elements.Add(element);
        InvalidateFocusCache();

        // Auto-focus only if this is the first focusable element
        if (_focusedElementIndex == -1 && element is { isVisible: true, isFocusable: true })
        {
            _focusedElementIndex = _elements.Count - 1;
            UpdateFocus();
        }
    }

    protected void RemoveElement(ITuiElement element)
    {
        var index = _elements.IndexOf(element);
        if (index >= 0)
        {
            _elements.RemoveAt(index);
            InvalidateFocusCache();

            if (index == _focusedElementIndex)
            {
                MoveFocusNext();
            }
            else if (index < _focusedElementIndex)
            {
                _focusedElementIndex--;
            }
        }
    }

    protected void ClearElements()
    {
        _elements.Clear();
        _focusedElementIndex = -1;
        InvalidateFocusCache();
    }

    protected void SetFocus(ITuiElement element)
    {
        if (element == null) return;

        var index = _elements.IndexOf(element);
        if (index >= 0 && element.isVisible && element.isFocusable)
        {
            _focusedElementIndex = index;
            UpdateFocus();
        }
    }

    protected void MoveFocusNext()
    {
        RebuildFocusCache();
        if (_focusableElements.Count == 0)
        {
            _focusedElementIndex = -1;
            UpdateFocus();
            return;
        }

        var currentFocused = _focusedElementIndex >= 0 && _focusedElementIndex < _elements.Count 
            ? _elements[_focusedElementIndex] 
            : null;

        var currentIndex = currentFocused != null 
            ? _focusableElements.IndexOf(currentFocused) 
            : -1;

        var nextIndex = (currentIndex + 1) % _focusableElements.Count;
        _focusedElementIndex = _elements.IndexOf(_focusableElements[nextIndex]);
        
        UpdateFocus();
    }

    protected void MoveFocusPrevious()
    {
        RebuildFocusCache();
        if (_focusableElements.Count == 0)
        {
            _focusedElementIndex = -1;
            UpdateFocus();
            return;
        }

        var currentFocused = _focusedElementIndex >= 0 && _focusedElementIndex < _elements.Count 
            ? _elements[_focusedElementIndex] 
            : null;

        var currentIndex = currentFocused != null 
            ? _focusableElements.IndexOf(currentFocused) 
            : -1;

        var nextIndex = currentIndex <= 0 ? _focusableElements.Count - 1 : currentIndex - 1;
        _focusedElementIndex = _elements.IndexOf(_focusableElements[nextIndex]);
        
        UpdateFocus();
    }

    private void UpdateFocus()
    {
        for (var i = 0; i < _elements.Count; i++)
        {
            var element = _elements[i];
            if (element != null)
            {
                try
                {
                    element.HasFocus = (i == _focusedElementIndex);
                }
                catch (Exception ex)
                {
                    TuiDiagnosticsService.Instance.Log($"Error setting focus for element {i}: {ex.Message}");
                }
            }
        }
    }
}