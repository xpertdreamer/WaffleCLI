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
    /// <summary>
    /// Console panel component for displaying and interacting with external processes
    /// </summary>
    public class ConsolePanel : FocusableComponentBase, IDisposable
    {
        private readonly List<string> _outputLines = new List<string>();
        private readonly StringBuilder _currentInput = new StringBuilder();
        private int _scrollOffset = 0;
        private IProcessRunner _processRunner;
        private bool _processRunning = false;
        private readonly object _outputLock = new object();
        private DateTime _lastBlink = DateTime.Now;
        private bool _cursorVisible = true;
        private int _cursorPosition = 0;

        public ColorScheme NormalColors { get; set; } = new ColorScheme(ConsoleColor.White, ConsoleColor.Black);
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.White);
        public ColorScheme ErrorColors { get; set; } = new ColorScheme(ConsoleColor.Red, ConsoleColor.Black);
        public ColorScheme InputColors { get; set; } = new ColorScheme(ConsoleColor.Green, ConsoleColor.Black);
        
        public string Prompt { get; set; } = "> ";
        public int MaxHistoryLines { get; set; } = 1000;
        public bool ShowPrompt { get; set; } = true;

        public ConsolePanel(string id) : base(id)
        {
            Width = 60;
            Height = 20;
            
            // Add welcome message
            _outputLines.Add("Console Panel Ready");
            _outputLines.Add("Use 'start <command>' to launch a process");
            _outputLines.Add("Type 'exit' to close process, 'clear' to clear console");
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
                    AddOutputLine("  help               - Show this help");
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
                _scrollOffset = Math.Max(0, _outputLines.Count - (Height - 2));
            }
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            var colors = HasFocus ? FocusColors : NormalColors;
            var borderStyle = HasFocus ? BorderStyle.Double : BorderStyle.Single;
            
            // Draw border
            renderEngine.DrawBox(X, Y, Width, Height, borderStyle, colors);

            // Draw output lines
            int visibleLines = Math.Max(0, Height - 2);
            lock (_outputLock)
            {
                for (int i = 0; i < visibleLines; i++)
                {
                    int lineIndex = _scrollOffset + i;
                    if (lineIndex >= 0 && lineIndex < _outputLines.Count)
                    {
                        int lineY = Y + 1 + i;
                        string line = _outputLines[lineIndex];
                        
                        // Truncate line if too long
                        if (line.Length > Width - 2)
                        {
                            line = line.Substring(0, Width - 2);
                        }
                        
                        renderEngine.DrawString(X + 1, lineY, line, colors);
                    }
                }
            }

            // Draw input line if focused
            if (HasFocus)
            {
                DrawInputLine(renderEngine);
            }
        }

        private void DrawInputLine(IRenderEngine renderEngine)
        {
            int inputY = Y + Height - 1;
            string inputDisplay = ShowPrompt ? Prompt : "";
            inputDisplay += _currentInput.ToString();
            
            // Truncate if too long
            if (inputDisplay.Length > Width - 2)
            {
                inputDisplay = inputDisplay.Substring(inputDisplay.Length - (Width - 2));
            }
            
            // Draw input background
            renderEngine.FillRectangle(X + 1, inputY, Width - 2, 1, ' ', InputColors);
            
            // Draw input text
            renderEngine.DrawString(X + 1, inputY, inputDisplay, InputColors);
            
            // Draw cursor
            if (_cursorVisible && HasFocus)
            {
                int cursorX = X + 1 + (ShowPrompt ? Prompt.Length : 0) + _cursorPosition;
                if (cursorX < X + Width - 1)
                {
                    renderEngine.DrawChar(cursorX, inputY, '_', 
                        new ColorScheme(InputColors.Background, InputColors.Foreground));
                }
            }
        }

        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) return false;

            // Handle common navigation (Tab, Escape)
            if (HandleCommonNavigation(inputEvent))
                return true;

            switch (inputEvent.Key)
            {
                case ConsoleKey.Enter:
                    if (_currentInput.Length > 0)
                    {
                        string input = _currentInput.ToString();
                        _currentInput.Clear();
                        _cursorPosition = 0;
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
                    // Scroll output up
                    lock (_outputLock)
                    {
                        if (_scrollOffset > 0)
                        {
                            _scrollOffset--;
                        }
                    }
                    return true;

                case ConsoleKey.DownArrow:
                    // Scroll output down
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
            
            // Blink cursor
            if ((DateTime.Now - _lastBlink).TotalMilliseconds > 500)
            {
                _cursorVisible = !_cursorVisible;
                _lastBlink = DateTime.Now;
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
            _cursorVisible = true;
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