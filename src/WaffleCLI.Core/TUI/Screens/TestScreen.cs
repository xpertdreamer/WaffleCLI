using WaffleCLI.Core.TUI.Elements;

namespace WaffleCLI.Core.TUI.Screens;

public class TestScreen : BasicTuiScreen
{
    private TextElement _statusElement;
    private ButtonElement _testButton;
    private TextElement _debugInfo;
    private int _clickCount = 0;

    public override string Title => "Test Screen - Debug";

    public override async Task InitializeAsync()
    {
        ClearElements();

        _statusElement = new TextElement
        {   
            X = 2, 
            Y = 2, 
            Width = 50,
            Text = "Test Screen Loaded - Elements should be visible below",
            Color = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkBlue,
            isFocusable = false,
            isVisible = true
        };

        _testButton = new ButtonElement
        {
            X = 2,
            Y = 5,
            Width = 20,
            Height = 3,
            Text = "Click Me!",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkCyan,
            FocusColor = ConsoleColor.Black,
            FocusBackgroundColor = ConsoleColor.Yellow,
            isFocusable = true,
            isVisible = true
        };

        _debugInfo = new TextElement
        {
            X = 2,
            Y = 9,
            Width = 60,
            Height = 10,
            Text = "Debug Info:\n- Screen initialized\n- Waiting for input...",
            Color = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.Black,
            HasBorder = true,
            isFocusable = false,
            isVisible = true
        };

        _testButton.Clicked += OnTestButtonClicked;

        AddElement(_statusElement);
        AddElement(_testButton);
        AddElement(_debugInfo);

        await base.InitializeAsync();

        UpdateDebugInfo("Initialization complete");
    }

    private void OnTestButtonClicked()
    {
        _clickCount++;
        UpdateDebugInfo($"Button clicked {_clickCount} times!\n" +
                       $"Screen size: {Console.WindowWidth}x{Console.WindowHeight}\n" +
                       $"Focus index: {_focusedElementIndex}\n" +
                       $"Elements count: {_elements.Count}");
    }

    private void UpdateDebugInfo(string message)
    {
        if (_debugInfo != null)
        {
            _debugInfo.Text = $"Debug Info:\n- {DateTime.Now:HH:mm:ss.fff}\n- {message}";
        }
    }

    protected override void RecalculateLayout()
    {
        Console.Write($"TestScreen.RecalculateLayout() called - Window: {Console.WindowWidth}x{Console.WindowHeight}");

        var screenWidth = Console.WindowWidth;
        var screenHeight = Console.WindowHeight;

        if (_statusElement != null)
        {
            _statusElement.Width = Math.Max(10, screenWidth - 4);
            _statusElement.Text = $"Test Screen - Size: {screenWidth}x{screenHeight} - Clicks: {_clickCount}";
        }

        if (_testButton != null)
        {
            _testButton.X = Math.Max(2, (screenWidth - _testButton.Width) / 2);
        }

        if (_debugInfo != null)
        {
            _debugInfo.Width = Math.Max(10, screenWidth - 4);
            _debugInfo.Height = Math.Max(5, screenHeight - 12);
        }

        UpdateDebugInfo($"Layout recalculated\nWindow: {screenWidth}x{screenHeight}");
    }

    public override async Task HandleInputAsync(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.F1:
                UpdateDebugInfo("F1 pressed - Adding new element");
                AddNewTestElement();
                break;

            case ConsoleKey.F2:
                UpdateDebugInfo("F2 pressed - Toggling element visibility");
                ToggleElementVisibility();
                break;

            case ConsoleKey.F3:
                UpdateDebugInfo("F3 pressed - Testing focus");
                CycleFocus();
                break;

            case ConsoleKey.F5:
                UpdateDebugInfo("F5 pressed - Force redraw");
                _needsLayoutRecalculation = true;
                break;
        }

        await base.HandleInputAsync(keyInfo);
    }

    private void AddNewTestElement()
    {
        var newElement = new TextElement
        {
            X = 2,
            Y = 12 + (_elements.Count * 2),
            Width = 30,
            Height = 1,
            Text = $"Dynamic Element #{_elements.Count}",
            Color = ConsoleColor.Cyan,
            BackgroundColor = ConsoleColor.DarkMagenta,
            isFocusable = false,
            isVisible = true
        };

        AddElement(newElement);
        UpdateDebugInfo($"Added new element - Total: {_elements.Count}");
    }

    private void ToggleElementVisibility()
    {
        if (_elements.Count > 1)
        {
            var element = _elements[1]; // Второй элемент (первый - статус)
            element.isVisible = !element.isVisible;
            UpdateDebugInfo($"Toggled visibility of element 1: {element.isVisible}");
        }
    }

    private void CycleFocus()
    {
        var focusableElements = _elements.Where(e => e.isFocusable).ToList();
        if (focusableElements.Count > 0)
        {
            MoveFocusNext();
            UpdateDebugInfo($"Focus cycled - Focused index: {_focusedElementIndex}");
        }
    }

    public override async Task RenderAsync()
    {
        await base.RenderAsync();

        UpdateDebugInfo($"Render completed\nLayout recalc: {_needsLayoutRecalculation}\nFirst render: {_firstRender}");
        
    }
}