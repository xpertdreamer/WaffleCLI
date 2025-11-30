using System.Text;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Elements;
using WaffleCLI.Core.TUI.Rendering;
using WaffleCLI.Core.TUI.Diagnostics;

namespace WaffleCLI.Core.TUI.Screens;

public class DiagnosticScreen : BasicTuiScreen
{
    private TextElement _statusElement;
    private ButtonElement _testButton;
    private TextElement _logElement;
    private TextElement _stateElement;
    private int _frameCount = 0;
    private int _clickCount = 0;
    private DateTime _startTime;
    private readonly List<string> _logMessages = new();
    private readonly int _maxLogLines = 10;

    public override string Title => "TUI Diagnostics";

    public override async Task InitializeAsync()
    {
        TuiDiagnosticsService.Instance.Log("DiagnosticScreen.InitializeAsync started");

        _startTime = DateTime.Now;

        try
        {
            ClearElements();

            // Создаем элементы
            _statusElement = new TextElement
            {
                X = 2,
                Y = 2,
                Width = 70,
                Text = "Diagnostics Active - Press F1-F5 for tests, Tab to navigate",
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
                Text = "Test Button",
                Color = ConsoleColor.White,
                BackgroundColor = ConsoleColor.DarkCyan,
                FocusColor = ConsoleColor.Black,
                FocusBackgroundColor = ConsoleColor.Yellow,
                isFocusable = true,
                isVisible = true
            };

            _logElement = new TextElement
            {
                X = 25,
                Y = 5,
                Width = 45,
                Height = 8,
                Text = "Event Log:\nWaiting for events...",
                Color = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.Black,
                HasBorder = true,
                isFocusable = false,
                isVisible = true
            };

            _stateElement = new TextElement
            {
                X = 2,
                Y = 9,
                Width = 70,
                Height = 10,
                Text = "System State:\nInitializing...",
                Color = ConsoleColor.Cyan,
                BackgroundColor = ConsoleColor.DarkGray,
                HasBorder = true,
                isFocusable = false,
                isVisible = true
            };

            // Подписываемся на события
            _testButton.Clicked += OnTestButtonClicked;

            // Добавляем элементы
            AddElement(_statusElement);
            AddElement(_testButton);
            AddElement(_logElement);
            AddElement(_stateElement);

            // Базовая инициализация ДО установки фокуса
            await base.InitializeAsync();

            // Устанавливаем фокус только после успешной инициализации
            if (_testButton != null && _elements.Contains(_testButton))
            {
                SetFocus(_testButton);
                LogEvent("ButtonElement focused during initialization");
            }
            else
            {
                LogEvent("Warning: ButtonElement not available for focus");
            }

            UpdateState();
            LogEvent("Screen initialized successfully");

            TuiDiagnosticsService.Instance.Log("DiagnosticScreen.InitializeAsync completed");
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"InitializeAsync failed: {ex}");
            LogEvent($"Initialization failed: {ex.Message}");
            throw;
        }
    }

    public override async Task RenderAsync()
{
    _frameCount++;
    
    try
    {
        // Принудительно обновляем состояние перед рендером
        UpdateState();
        
        await base.RenderAsync();
        
        // Логируем состояние ButtonElement
        var buttonElement = _elements.OfType<ButtonElement>().FirstOrDefault();
        if (buttonElement != null)
        {
            TuiDiagnosticsService.Instance.Log($"ButtonElement state - Visible: {buttonElement.isVisible}, Focusable: {buttonElement.isFocusable}, HasFocus: {buttonElement.HasFocus}");
        }
        
        if (_frameCount % 30 == 0) // Логируем каждые 30 кадров
        {
            LogEvent($"Frame {_frameCount} rendered - Button visible: {buttonElement?.isVisible ?? false}");
        }
    }
    catch (Exception ex)
    {
        TuiDiagnosticsService.Instance.Log($"RenderAsync error: {ex}");
    }
}

    protected override void RecalculateLayout()
    {
        TuiDiagnosticsService.Instance.Log("RecalculateLayout called");

        try
        {
            var screenWidth = Console.WindowWidth;
            var screenHeight = Console.WindowHeight;

            // Обновляем размеры элементов
            if (_statusElement != null)
            {
                _statusElement.Width = Math.Max(10, screenWidth - 4);
                _statusElement.X = 2;
            }

            // Убеждаемся, что ButtonElement всегда видим и получает правильные координаты
            var buttonElement = _elements.OfType<ButtonElement>().FirstOrDefault();
            if (buttonElement != null)
            {
                buttonElement.X = Math.Max(2, (screenWidth - buttonElement.Width) / 2);
                buttonElement.Y = 5;
                buttonElement.isVisible = true; // Принудительно делаем видимым
                TuiDiagnosticsService.Instance.Log(
                    $"ButtonElement repositioned to ({buttonElement.X}, {buttonElement.Y})");
            }

            if (_logElement != null)
            {
                _logElement.Width = Math.Max(20, screenWidth - 30);
                _logElement.Height = Math.Max(5, screenHeight - 15);
            }

            if (_stateElement != null)
            {
                _stateElement.Width = Math.Max(10, screenWidth - 4);
                _stateElement.Height = Math.Max(5, screenHeight - 20);
                _stateElement.Y = _logElement?.Y + (_logElement?.Height ?? 0) + 1 ?? 10;
            }

            UpdateState();
            LogEvent($"Layout recalculated: {screenWidth}x{screenHeight}");
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"RecalculateLayout error: {ex}");
        }
    }

    public override async Task HandleInputAsync(ConsoleKeyInfo keyInfo)
    {
        LogEvent($"Input received: {keyInfo.Key} (Modifiers: {keyInfo.Modifiers})");

        // Сохраняем предыдущее состояние фокуса
        var oldFocusedElementIndex = _focusedElementIndex;

        // Диагностические команды
        switch (keyInfo.Key)
        {
            case ConsoleKey.F1:
                LogEvent("F1 - Toggle element visibility");
                ToggleElementVisibility();
                break;

            case ConsoleKey.F2:
                LogEvent("F2 - Add test element");
                AddTestElement();
                break;

            case ConsoleKey.F3:
                LogEvent("F3 - Test focus system");
                TestFocusSystem();
                break;

            case ConsoleKey.F4:
                LogEvent("F4 - Force garbage collection");
                GC.Collect();
                LogEvent("Garbage collection forced");
                break;

            case ConsoleKey.F5:
                LogEvent("F5 - Dump diagnostics");
                DumpDiagnostics();
                break;

            case ConsoleKey.Tab:
                LogEvent("Tab - Focus navigation");
                // Позволяем базовому классу обработать навигацию
                break;
        }

        try
        {
            await base.HandleInputAsync(keyInfo);

            // Логируем изменение фокуса
            if (oldFocusedElementIndex != _focusedElementIndex)
            {
                LogEvent($"Focus changed from {oldFocusedElementIndex} to {_focusedElementIndex}");

                var focusedElement = _focusedElementIndex >= 0 && _focusedElementIndex < _elements.Count
                    ? _elements[_focusedElementIndex]
                    : null;

                if (focusedElement is ButtonElement)
                {
                    LogEvent("ButtonElement now has focus!");
                }
            }

            UpdateState();
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"HandleInputAsync error: {ex}");
            LogEvent($"Input handling error: {ex.Message}");
        }
    }

    public override async Task HandleResizeAsync()
    {
        LogEvent("Screen resize detected");
        
        try
        {
            await base.HandleResizeAsync();
            UpdateState();
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"HandleResizeAsync error: {ex}");
        }
    }

    private void OnTestButtonClicked()
    {
        _clickCount++;
        LogEvent($"Button clicked {_clickCount} times");
        UpdateState();
    }

    private void LogEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _logMessages.Add($"{timestamp}: {message}");
        
        while (_logMessages.Count > _maxLogLines)
        {
            _logMessages.RemoveAt(0);
        }

        if (_logElement != null)
        {
            _logElement.Text = "Event Log:\n" + string.Join("\n", _logMessages);
        }
    }

    private void UpdateState()
    {
        try
        {
            var stateText = new StringBuilder();
            stateText.AppendLine("System State:");
            stateText.AppendLine($"Uptime: {(DateTime.Now - _startTime):hh\\:mm\\:ss}");
            stateText.AppendLine($"Frames: {_frameCount}");
            stateText.AppendLine($"Clicks: {_clickCount}");
            stateText.AppendLine($"Focus Index: {_focusedElementIndex}");
            stateText.AppendLine($"Elements: {_elements.Count}");
            stateText.AppendLine($"Screen: {Console.WindowWidth}x{Console.WindowHeight}");
            stateText.AppendLine($"Buffer: {RenderEngine?.Width}x{RenderEngine?.Height}");
            stateText.AppendLine($"Visible: {_elements.Count(e => e.isVisible)}");
            stateText.AppendLine($"Focusable: {_elements.Count(e => e.isFocusable)}");

            if (_stateElement != null)
            {
                _stateElement.Text = stateText.ToString();
            }
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"UpdateState error: {ex}");
        }
    }

    private void ToggleElementVisibility()
    {
        if (_elements.Count > 1)
        {
            var element = _elements[1];
            element.isVisible = !element.isVisible;
            LogEvent($"Toggled element visibility: {element.isVisible}");
            UpdateState();
        }
    }

    private void AddTestElement()
    {
        try
        {
            var newElement = new TextElement
            {
                X = 45,
                Y = 15 + (_elements.Count * 2),
                Width = 25,
                Height = 1,
                Text = $"Dynamic Element #{_elements.Count}",
                Color = ConsoleColor.Magenta,
                BackgroundColor = ConsoleColor.DarkYellow,
                isFocusable = false,
                isVisible = true
            };

            AddElement(newElement);
            LogEvent($"Added dynamic element #{_elements.Count}");
            UpdateState();
        }
        catch (Exception ex)
        {
            LogEvent($"Failed to add element: {ex.Message}");
        }
    }

    private void TestFocusSystem()
    {
        LogEvent("Testing focus system...");
        
        var focusableElements = _elements.Where(e => e.isFocusable).ToList();
        LogEvent($"Focusable elements: {focusableElements.Count}");
        
        if (focusableElements.Count > 0)
        {
            MoveFocusNext();
            LogEvent($"Focus moved to index: {_focusedElementIndex}");
        }
        
        UpdateState();
    }

    private void DumpDiagnostics()
    {
        try
        {
            var diagnostics = TuiDiagnosticsService.Instance.GetLog();
            var dumpFile = Path.Combine(Environment.CurrentDirectory, $"tui_dump_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            
            File.WriteAllText(dumpFile, diagnostics);
            LogEvent($"Diagnostics dumped to: {dumpFile}");
        }
        catch (Exception ex)
        {
            LogEvent($"Failed to dump diagnostics: {ex.Message}");
        }
    }
}