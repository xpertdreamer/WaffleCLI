using System.Text;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Processes;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Infrastructure.Logging;
using WaffleCLI.Core.TUI.Processes;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    public class ConsolePanel : FocusableComponentBase, IDisposable
    {
        private readonly List<string> _outputLines = [];
        private readonly StringBuilder _currentInput = new();
        private int _scrollOffset = 0;
        private IProcessRunner _processRunner;
        private bool _processRunning = false;
        private readonly object _outputLock = new object();
        private DateTime _lastBlink = DateTime.Now;
        private bool _cursorVisible = true;
        private int _cursorPosition = 0;
        private int _horizontalScroll = 0;
        private bool _wordWrap = true;
        private bool _isFullscreen = false;
        private readonly List<List<string>> _wrappedLines = [];

        public ColorScheme NormalColors { get; set; } = new ColorScheme(ConsoleColor.White, ConsoleColor.Black);
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.White);
        public ColorScheme ErrorColors { get; set; } = new ColorScheme(ConsoleColor.Red, ConsoleColor.Black);
        public ColorScheme InputColors { get; set; } = new ColorScheme(ConsoleColor.Green, ConsoleColor.Black);
        
        public bool IsFullscreen => _isFullscreen;
        public bool InputLine { get; set; } = true; 

        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                if (_wordWrap != value)
                {
                    _wordWrap = value;
                    lock (_outputLock)
                    {
                        _wrappedLines.Clear();
                    }
                }
            }
        }

        public int HorizontalScroll
        {
            get => _horizontalScroll;
            set => _horizontalScroll = Math.Max(0, value);
        }
        
        public string Prompt { get; set; } = "> ";
        private int MaxHistoryLines { get; set; } = 1000;
        public bool ShowPrompt { get; set; } = true;

        public ConsolePanel(string id) : base(id)
        {
            Width = 100;
            Height = 20;
    
            _outputLines.Add("Console Panel Ready");
            _outputLines.Add("Use 'start <command>' to launch a process");
            _outputLines.Add("Type 'exit' to close process, 'clear' to clear console");
            _outputLines.Add("Press Alt+F to toggle fullscreen mode");
            _outputLines.Add("Press Alt+Left/Right to scroll horizontally when word wrap is disabled");
        }

        /// <summary>
        /// Starts an external process
        /// </summary>
        public async void StartProcess(string fileName, string arguments = "", string workingDirectory = null)
        {
            try
            {
                if (_processRunner != null && _processRunner.IsRunning)
                {
                    AddOutputLine($"Process {_processRunner.ProcessId} is already running. Stop it first.");
                    return;
                }

                _processRunner?.Dispose();
                _processRunner = new ProcessRunner();
                
                _processRunner.OutputReceived += OnProcessOutputReceived;
                _processRunner.Exited += OnProcessExited;

                AddOutputLine($"Starting: {fileName} {arguments}");
                
                await _processRunner.StartAsync(fileName, arguments, workingDirectory);
                _processRunning = true;
                
                AddOutputLine($"Process started with PID: {_processRunner.ProcessId}");
            }
            catch (Exception ex)
            {
                AddOutputLine($"Failed to start process: {ex.Message}", isError: true);
                TuiLogger.LogError($"Failed to start process {fileName}", ex);
            }
        }

        /// <summary>
        /// Stops the running process
        /// </summary>
        public void StopProcess()
        {
            if (_processRunner != null && _processRunner.IsRunning)
            {
                AddOutputLine($"Stopping process {_processRunner.ProcessId}...");
                _processRunner.Kill();
                _processRunning = false;
            }
        }

        /// <summary>
        /// Sends input to the running process
        /// </summary>
        private async void SendInputToProcess(string input)
        {
            if (_processRunner != null && _processRunner.IsRunning)
            {
                try
                {
                    await _processRunner.SendLineAsync(input);
                    AddOutputLine($"{Prompt}{input}");
                }
                catch (Exception ex)
                {
                    AddOutputLine($"Failed to send input: {ex.Message}", isError: true);
                }
            }
            else
            {
                // Handle local commands when no process is running
                HandleLocalCommand(input);
            }
        }

        /// <summary>
        /// Handles local console commands
        /// </summary>
        private void HandleLocalCommand(string command)
        {
            var parts = command.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            var cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "fullscreen":
                case "fs":
                    ToggleFullscreen();
                    break;

                case "wordwrap":
                case "wrap":
                    _wordWrap = !_wordWrap;
                    _wrappedLines.Clear();
                    AddOutputLine($"Word wrap: {(_wordWrap ? "ENABLED" : "DISABLED")}");
                    AddOutputLine($"Use Ctrl+Left/Right to scroll horizontally when disabled");
                    break;

                case "scroll":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int scroll))
                    {
                        _horizontalScroll = Math.Max(0, scroll);
                        AddOutputLine($"Horizontal scroll set to: {_horizontalScroll}");
                    }
                    else
                    {
                        AddOutputLine($"Current horizontal scroll: {_horizontalScroll}");
                    }
                    break;

                case "start":
                    if (parts.Length > 1)
                    {
                        var fileArgs = parts[1].Split(' ', 2);
                        var fileName = fileArgs[0];
                        var arguments = fileArgs.Length > 1 ? fileArgs[1] : "";
                        StartProcess(fileName, arguments);
                    }
                    else
                    {
                        AddOutputLine("Usage: start <executable> [arguments]", isError: true);
                    }
                    break;

                case "clear":
                    ClearOutput();
                    break;

                case "exit":
                    StopProcess();
                    break;

                case "help":
                    AddOutputLine("Available commands:");
                    AddOutputLine("  start <exe> [args] - Start a process");
                    AddOutputLine("  clear              - Clear console");
                    AddOutputLine("  exit               - Stop current process");
                    AddOutputLine("  fullscreen / fs    - Toggle fullscreen mode");
                    AddOutputLine("  wordwrap / wrap    - Toggle word wrapping");
                    AddOutputLine("  scroll [num]       - Set horizontal scroll position");
                    AddOutputLine("  help               - Show this help");
                    AddOutputLine("");
                    AddOutputLine("Hotkeys:");
                    AddOutputLine("  Alt+F              - Toggle fullscreen");
                    AddOutputLine("  Ctrl+W             - Toggle word wrap");
                    AddOutputLine("  Alt+Left/Right     - Scroll horizontally (no wrap)");
                    AddOutputLine("  Ctrl+Home          - Reset horizontal scroll");
                    break;

                default:
                    AddOutputLine($"Unknown command: {cmd}. Type 'help' for available commands.", isError: true);
                    break;
            }
        }

        /// <summary>
        /// Clears the console output
        /// </summary>
        public void ClearOutput()
        {
            lock (_outputLock)
            {
                _outputLines.Clear();
                _scrollOffset = 0;
            }
        }

        /// <summary>
        /// Adds a line to the output
        /// </summary>
        public void AddOutputLine(string line, bool isError = false)
        {
            lock (_outputLock)
            {
                _outputLines.Add(line);
                
                // Trim history if needed
                while (_outputLines.Count > MaxHistoryLines)
                {
                    _outputLines.RemoveAt(0);
                }
                
                // Auto-scroll to bottom
                _scrollOffset = Math.Max(0, _outputLines.Count - (Height - (InputLine ? 2 : 1)));
            }
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            // If in fullscreen mode, render as overlay on top of everything
            if (_isFullscreen)
            {
                RenderFullscreenOverlay(renderEngine);
                return;
            }

            int absoluteX = AbsoluteX;
            int absoluteY = AbsoluteY;

            var colors = HasFocus ? FocusColors : NormalColors;
            var borderStyle = HasFocus ? BorderStyle.Double : BorderStyle.Single;

            renderEngine.DrawBox(absoluteX, absoluteY, Width, Height, borderStyle, colors);
            
            int visibleLines = Math.Max(0, Height - (InputLine ? 2 : 1));
            lock (_outputLock)
            {
                // Update wrapped lines if needed
                UpdateWrappedLines(Width - 2);

                for (int i = 0; i < visibleLines; i++)
                {
                    int lineIndex = _scrollOffset + i;
                    if (lineIndex >= 0 && lineIndex < _wrappedLines.Count)
                    {
                        int lineY = absoluteY + 1 + i;
                        var wrappedLine = _wrappedLines[lineIndex];

                        if (wrappedLine.Count > 0)
                        {
                            int segmentIndex = Math.Min(_horizontalScroll, wrappedLine.Count - 1);
                            string segment = wrappedLine[segmentIndex];

                            // Add ellipsis at the END if there are more segments and we're not at the beginning
                            if (wrappedLine.Count > 1 && segmentIndex > 0)
                            {
                                if (segment.Length > 0 && segment.Length >= 3)
                                {
                                    segment = segment.Substring(0, Math.Max(0, segment.Length - 3)) + "...";
                                }
                            }

                            // Ensure segment fits in available width
                            if (segment.Length > Width - 2)
                            {
                                segment = segment.Substring(0, Width - 2);
                            }

                            renderEngine.DrawString(absoluteX + 1, lineY, segment, colors);
                        }
                    }
                }
            }
            
            if (HasFocus && InputLine)
            {
                DrawInputLine(renderEngine, absoluteX, absoluteY, Width, Height);
            }
        }

        private void RenderFullscreenOverlay(IRenderEngine renderEngine)
        {
            // Get parent dimensions or console dimensions
            int screenWidth = Console.WindowWidth;
            int screenHeight = Console.WindowHeight;

            if (Parent is ComponentBase parent)
            {
                screenWidth = parent.Width;
                screenHeight = parent.Height;
            }

            // Fill entire screen with background
            renderEngine.FillRectangle(0, 0, screenWidth, screenHeight, ' ',
                new ColorScheme(ConsoleColor.White, ConsoleColor.Black));

            // Draw border around the entire screen
            renderEngine.DrawBox(0, 0, screenWidth, screenHeight, BorderStyle.Double,
                new ColorScheme(ConsoleColor.Cyan, ConsoleColor.Black));

            // Draw title
            string title = " CONSOLE PANEL (FULLSCREEN - Alt+F to exit) ";
            if (title.Length <= screenWidth - 4)
            {
                int titleX = (screenWidth - title.Length) / 2;
                renderEngine.DrawString(titleX, 0, title,
                    new ColorScheme(ConsoleColor.Yellow, ConsoleColor.Black));
            }
            
            int contentWidth = screenWidth - 2;
            int contentHeight = screenHeight - (InputLine ? 4 : 3); // Top border, title, bottom border, optional input line
            int contentY = 2;

            // Update wrapped lines for fullscreen width
            UpdateWrappedLines(contentWidth);

            // Render content
            lock (_outputLock)
            {
                for (int i = 0; i < contentHeight; i++)
                {
                    int lineIndex = _scrollOffset + i;
                    if (lineIndex >= 0 && lineIndex < _wrappedLines.Count)
                    {
                        int lineY = contentY + i;
                        var wrappedLine = _wrappedLines[lineIndex];

                        if (wrappedLine.Count > 0)
                        {
                            int segmentIndex = Math.Min(_horizontalScroll, wrappedLine.Count - 1);
                            string segment = wrappedLine[segmentIndex];

                            // Add ellipsis at the END if there are more segments and we're not at the beginning
                            if (wrappedLine.Count > 1 && segmentIndex > 0)
                            {
                                if (segment.Length > 0 && segment.Length >= 3)
                                {
                                    segment = segment.Substring(0, Math.Max(0, segment.Length - 3)) + "...";
                                }
                            }

                            // Ensure segment fits in available width
                            if (segment.Length > contentWidth)
                            {
                                segment = segment.Substring(0, contentWidth);
                            }

                            renderEngine.DrawString(1, lineY, segment,
                                new ColorScheme(ConsoleColor.White, ConsoleColor.Black));
                        }
                    }
                }
            }
            
            if (InputLine)
            {
                DrawFullscreenInputLine(renderEngine, screenWidth, screenHeight);
            }
        }

        private void DrawFullscreenInputLine(IRenderEngine renderEngine, int screenWidth, int screenHeight)
        {
            int inputY = screenHeight - 2;
            string inputDisplay = ShowPrompt ? Prompt : "";
            inputDisplay += _currentInput.ToString();

            int maxDisplayWidth = screenWidth - 4;
            if (inputDisplay.Length > maxDisplayWidth)
            {
                // Show the BEGINNING of input when typing (not the end)
                inputDisplay = inputDisplay.Substring(0, maxDisplayWidth);

                // Add ellipsis at the end to indicate truncation
                if (inputDisplay.Length >= 3)
                {
                    inputDisplay = inputDisplay.Substring(0, inputDisplay.Length - 3) + "...";
                }
            }

            // Draw input background
            renderEngine.FillRectangle(2, inputY, screenWidth - 4, 1, ' ',
                new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray));

            // Draw input text
            renderEngine.DrawString(2, inputY, inputDisplay,
                new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray));

            // Draw cursor
            if (_cursorVisible && HasFocus)
            {
                int cursorX = 2 + (ShowPrompt ? Prompt.Length : 0) + _cursorPosition;
                if (cursorX < screenWidth - 2)
                {
                    renderEngine.DrawChar(cursorX, inputY, '_',
                        new ColorScheme(ConsoleColor.Gray, ConsoleColor.Black));
                }
            }
        }

        private void DrawInputLine(IRenderEngine renderEngine, int panelX, int panelY, int panelWidth, int panelHeight)
        {
            int inputY = panelY + panelHeight - 1;
            string inputDisplay = ShowPrompt ? Prompt : "";
            inputDisplay += _currentInput.ToString();
    
            // Handle horizontal scrolling for input
            int maxDisplayWidth = panelWidth - 2;
            if (inputDisplay.Length > maxDisplayWidth)
            {
                // Show the end of input when typing
                int startIndex = Math.Max(0, inputDisplay.Length - maxDisplayWidth);
                inputDisplay = inputDisplay.Substring(startIndex);
        
                // Add ellipsis if we're showing truncated text
                if (startIndex > 0)
                {
                    inputDisplay = "..." + inputDisplay;
                }
            }
    
            renderEngine.FillRectangle(panelX + 1, inputY, panelWidth - 2, 1, ' ', InputColors);
            renderEngine.DrawString(panelX + 1, inputY, inputDisplay, InputColors);
    
            if (_cursorVisible && HasFocus)
            {
                int cursorX = panelX + 1 + (ShowPrompt ? Prompt.Length : 0) + _cursorPosition;
                // Adjust cursor position for truncated display
                if (inputDisplay.Length < _currentInput.Length + (ShowPrompt ? Prompt.Length : 0))
                {
                    cursorX = panelX + 1 + inputDisplay.Length - (_currentInput.Length - _cursorPosition);
                }
        
                if (cursorX < panelX + panelWidth - 1)
                {
                    renderEngine.DrawChar(cursorX, inputY, '_', 
                        new ColorScheme(InputColors.Background, InputColors.Foreground));
                }
            }
        }
        
        private void UpdateWrappedLines(int maxWidth)
        {
            if (maxWidth <= 0) return;
    
            lock (_outputLock)
            {
                // Clear wrapped lines and rebuild if needed
                if (_wrappedLines.Count != _outputLines.Count)
                {
                    _wrappedLines.Clear();
                    foreach (var line in _outputLines)
                    {
                        if (_wordWrap && maxWidth > 0)
                        {
                            _wrappedLines.Add(WrapLine(line, maxWidth));
                        }
                        else
                        {
                            _wrappedLines.Add(new List<string> { line });
                        }
                    }
                }
            }
        }

        private List<string> WrapLine(string line, int maxWidth)
        {
            var result = new List<string>();
    
            if (string.IsNullOrEmpty(line) || maxWidth <= 0)
            {
                result.Add(line);
                return result;
            }
    
            // Always start from the beginning of the line
            int currentPos = 0;
            while (currentPos < line.Length)
            {
                int chunkLength = Math.Min(maxWidth, line.Length - currentPos);
                string chunk = line.Substring(currentPos, chunkLength);
                result.Add(chunk);
                currentPos += chunkLength;
            }
    
            return result;
        }
        
        public void ToggleFullscreen()
        {
            _isFullscreen = !_isFullscreen;
    
            if (_isFullscreen)
            {
                // Store original dimensions
                if (Parent is ComponentBase parent)
                {
                    // In fullscreen mode, we'll use parent's dimensions during render
                    AddOutputLine("Entering fullscreen mode (Alt+F to exit)");
                }
            }
            else
            {
                AddOutputLine("Exiting fullscreen mode");
            }
    
            // Clear wrapped lines to force re-wrapping with new dimensions
            lock (_outputLock)
            {
                _wrappedLines.Clear();
                _horizontalScroll = 0;
            }
        }

        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) return false;

            // Handle common navigation (Tab, Escape)
            if (HandleCommonNavigation(inputEvent))
                return true;
            
            if (!InputLine)
            {
                switch (inputEvent.Key)
                {
                    case ConsoleKey.F when inputEvent.Modifiers.HasFlag(KeyModifiers.Alt):
                        ToggleFullscreen();
                        return true;

                    case ConsoleKey.UpArrow:
                        lock (_outputLock)
                        {
                            if (_scrollOffset > 0) _scrollOffset--;
                        }
                        return true;

                    case ConsoleKey.DownArrow:
                        lock (_outputLock)
                        {
                            if (_scrollOffset < Math.Max(0, _outputLines.Count - (Height - 1)))
                            {
                                _scrollOffset++;
                            }
                        }
                        return true;

                    case ConsoleKey.PageUp:
                        lock (_outputLock)
                        {
                            _scrollOffset = Math.Max(0, _scrollOffset - (Height - 1));
                        }
                        return true;

                    case ConsoleKey.PageDown:
                        lock (_outputLock)
                        {
                            int maxScroll = Math.Max(0, _outputLines.Count - (Height - 1));
                            _scrollOffset = Math.Min(maxScroll, _scrollOffset + (Height - 1));
                        }
                        return true;
                }
                return false; // Не обрабатываем остальные клавиши, если InputLine = false
            }

            // Обработка клавиш когда InputLine = true
            switch (inputEvent.Key)
            {
                case ConsoleKey.F when inputEvent.Modifiers.HasFlag(KeyModifiers.Alt):
                    ToggleFullscreen();
                    return true;

                case ConsoleKey.W when inputEvent.Modifiers.HasFlag(KeyModifiers.Control):
                    _wordWrap = !_wordWrap;
                    _wrappedLines.Clear(); // Force re-wrapping
                    AddOutputLine($"Word wrap: {(_wordWrap ? "ON" : "OFF")}");
                    return true;

                case ConsoleKey.A when inputEvent.Modifiers.HasFlag(KeyModifiers.Alt) && !_wordWrap:
                    if (_horizontalScroll > 0)
                    {
                        _horizontalScroll--;
                        AddOutputLine($"Horizontal scroll: {_horizontalScroll}");
                    }
                    return true;

                case ConsoleKey.D when inputEvent.Modifiers.HasFlag(KeyModifiers.Alt) && !_wordWrap:
                    lock (_outputLock)
                    {
                        if (_wrappedLines.Count > 0 && _scrollOffset < _wrappedLines.Count)
                        {
                            var currentLine = _wrappedLines[Math.Min(_scrollOffset, _wrappedLines.Count - 1)];
                            if (_horizontalScroll < currentLine.Count - 1)
                            {
                                _horizontalScroll++;
                                AddOutputLine($"Horizontal scroll: {_horizontalScroll}");
                            }
                        }
                    }
                    return true;

                case ConsoleKey.Home when inputEvent.Modifiers.HasFlag(KeyModifiers.Control):
                    _horizontalScroll = 0;
                    return true;

                case ConsoleKey.Enter:
                    if (_currentInput.Length > 0)
                    {
                        string input = _currentInput.ToString();
                        _currentInput.Clear();
                        _cursorPosition = 0;
                        _horizontalScroll = 0; // Reset horizontal scroll on new command
                        SendInputToProcess(input);
                    }
                    return true;

                case ConsoleKey.Backspace:
                    if (_cursorPosition > 0 && _currentInput.Length > 0)
                    {
                        _currentInput.Remove(_cursorPosition - 1, 1);
                        _cursorPosition--;
                    }
                    return true;

                case ConsoleKey.Delete:
                    if (_cursorPosition < _currentInput.Length)
                    {
                        _currentInput.Remove(_cursorPosition, 1);
                    }
                    return true;

                case ConsoleKey.LeftArrow:
                    if (_cursorPosition > 0)
                    {
                        _cursorPosition--;
                    }
                    return true;

                case ConsoleKey.RightArrow:
                    if (_cursorPosition < _currentInput.Length)
                    {
                        _cursorPosition++;
                    }
                    return true;

                case ConsoleKey.Home:
                    _cursorPosition = 0;
                    return true;

                case ConsoleKey.End:
                    _cursorPosition = _currentInput.Length;
                    return true;

                case ConsoleKey.UpArrow:
                    lock (_outputLock)
                    {
                        if (_scrollOffset > 0)
                        {
                            _scrollOffset--;
                        }
                    }
                    return true;

                case ConsoleKey.DownArrow:
                    lock (_outputLock)
                    {
                        if (_scrollOffset < Math.Max(0, _outputLines.Count - (Height - 2)))
                        {
                            _scrollOffset++;
                        }
                    }
                    return true;

                case ConsoleKey.PageUp:
                    lock (_outputLock)
                    {
                        _scrollOffset = Math.Max(0, _scrollOffset - (Height - 2));
                    }
                    return true;

                case ConsoleKey.PageDown:
                    lock (_outputLock)
                    {
                        int maxScroll = Math.Max(0, _outputLines.Count - (Height - 2));
                        _scrollOffset = Math.Min(maxScroll, _scrollOffset + (Height - 2));
                    }
                    return true;

                default:
                    // Handle printable characters
                    if (!char.IsControl(inputEvent.Character) &&
                        inputEvent.Character >= 32)
                    {
                        _currentInput.Insert(_cursorPosition, inputEvent.Character);
                        _cursorPosition++;
                        return true;
                    }
                    break;
            }

            return false;
        }

        public override void Update()
        {
            base.Update();
            
            if (InputLine && HasFocus)
            {
                if ((DateTime.Now - _lastBlink).TotalMilliseconds > 500)
                {
                    _cursorVisible = !_cursorVisible;
                    _lastBlink = DateTime.Now;
                }
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
            if (InputLine)
            {
                _cursorVisible = true;
            }
            TuiLogger.LogInfo($"ConsolePanel {Id} received focus");
        }

        public override void OnBlur()
        {
            base.OnBlur();
            TuiLogger.LogInfo($"ConsolePanel {Id} lost focus");
        }

        private void OnProcessOutputReceived(object sender, ProcessOutputEventArgs e)
        {
            AddOutputLine(e.Output, e.IsError);
        }

        private void OnProcessExited(object sender, ProcessExitedEventArgs e)
        {
            _processRunning = false;
            AddOutputLine($"Process exited with code: {e.ExitCode}");
    
            // Clean up
            if (_processRunner != null)
            {
                _processRunner.OutputReceived -= OnProcessOutputReceived;
                _processRunner.Exited -= OnProcessExited;
                _processRunner.Dispose();
                _processRunner = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopProcess();
                _processRunner?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}