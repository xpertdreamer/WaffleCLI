using System.Diagnostics;
using System.Text;
using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Process;

public class ProcessRunnerComponent : IProcessComponent
{
    private System.Diagnostics.Process _process;
    private readonly StringBuilder _outputBuffer = new();
    private readonly StringBuilder _errorBuffer = new();
    private readonly List<string> _outputHistory = [];
    private readonly List<string> _inputHistory = [];
    private int _scrollOffset = 0;
    private int _inputHistoryIndex = -1;
    private string _currentInput = string.Empty;

    public ProcessRunnerComponent(ProcessInfo processInfo)
    {
        ProcessInfo =  processInfo;
        Id = Guid.NewGuid().ToString();
    }

    public string Id { get; }

    ProcessState IProcessComponent.State { get; }

    public ComponentState State { get; private set; } = ComponentState.Created;
    public ProcessInfo ProcessInfo { get; }
    public ProcessState ProcessState { get; private set; } = ProcessState.NotStarted;
    
    public string Output => _outputBuffer.ToString();
    public string Error => _errorBuffer.ToString();
    
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 80;
    public int Height { get; set; } = 24;
    public bool isVisible { get; set; } = true;
    public bool isFocusable { get; set; } = true;
    public bool HasFocus { get; set; }
    
    public event Action<string>? OutputReceived;
    public event Action<string>? ErrorReceived;
    public event Action<int>? ProcessExited;
    public event Action? RequestRedraw;

    public async Task StartAsync()
    {
        if (ProcessState != ProcessState.NotStarted) return;

        try
        {
            _process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ProcessInfo.FileName,
                    Arguments = ProcessInfo.Arguments,
                    WorkingDirectory = string.IsNullOrEmpty(ProcessInfo.WorkingDirectory)
                        ? Directory.GetCurrentDirectory()
                        : ProcessInfo.WorkingDirectory,
                    UseShellExecute = ProcessInfo.UseShellExecute,
                    RedirectStandardInput = !ProcessInfo.UseShellExecute,
                    RedirectStandardOutput = !ProcessInfo.UseShellExecute,
                    RedirectStandardError = !ProcessInfo.UseShellExecute,
                    CreateNoWindow = !ProcessInfo.UseShellExecute,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };

            if (ProcessInfo.EnviromentVariables != null)
            {
                foreach (var env in ProcessInfo.EnviromentVariables)
                {
                    _process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
                }
            }

            if (!ProcessInfo.UseShellExecute)
            {
                _process.OutputDataReceived += OnOutputDataReceived;
                _process.ErrorDataReceived += OnErrorDataReceived;
            }

            _process.Exited += OnProcessExited;

            ProcessState = ProcessState.Running;
            _process.Start();

            if (!ProcessInfo.UseShellExecute)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            await OnRenderAsync();
        }
        catch (Exception ex)
        {
            ProcessState = ProcessState.Error;
            AddOutputLine($"Error starting process: {ex.Message}");
        }
    }
    
    public async Task StopAsync()
    {
        if (_process == null || _process.HasExited)
            return;

        try
        {
            if (!_process.WaitForExit(1000))
            {
                _process.Kill();
            }
        }
        catch (Exception ex)
        {
            AddOutputLine($"Error stopping process: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    public async Task WriteInputAsync(string input)
    {
        if (_process == null || _process.HasExited || ProcessInfo.UseShellExecute) return;

        try
        {
            await _process.StandardInput.WriteLineAsync(input);
            AddInputLine(input);
        }
        catch (Exception ex)
        {
            AddOutputLine($"Error writing input: {ex.Message}");
        }
    }
    
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            AddOutputLine(e.Data);
        }
    }
    
    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            AddErrorLine(e.Data);
        }
    }
    
    private void OnProcessExited(object sender, EventArgs e)
    {
        ProcessState = ProcessState.Exited;
        AddOutputLine($"\nProcess exited with code: {_process.ExitCode}");
        ProcessExited?.Invoke(_process.ExitCode);
        RequestRedraw?.Invoke();
    }

    private void AddOutputLine(string line)
    {
        _outputBuffer.AppendLine(line);
        _outputHistory.Add(line);

        if (_scrollOffset + (Height - 2) < _outputHistory.Count)
        {
            _scrollOffset = _outputHistory.Count - (Height - 2);
        }
        
        OutputReceived?.Invoke(line);
        RequestRedraw?.Invoke();
    }
    
    private void AddErrorLine(string line)
    {
        _errorBuffer.AppendLine(line);
        _outputHistory.Add($"[ERROR] {line}");
        
        if (_scrollOffset + (Height - 2) < _outputHistory.Count)
        {
            _scrollOffset = _outputHistory.Count - (Height - 2);
        }
        
        ErrorReceived?.Invoke(line);
        RequestRedraw?.Invoke();
    }
    
    private void AddInputLine(string input)
    {
        _inputHistory.Add(input);
        _inputHistoryIndex = _inputHistory.Count;
        _currentInput = string.Empty;
        RequestRedraw?.Invoke();
    }
    
    public async Task OnCreateAsync()
    {
        State = ComponentState.Created;
        await StartAsync();
    }

    public async Task OnRenderAsync()
    {
        Render();
        await Task.CompletedTask;
    }

    public void Render()
    {
        if (!isVisible) return;

        RenderHeader();
        RenderOutput();
        if (ProcessState == ProcessState.Running && !ProcessInfo.UseShellExecute)
        {
            RenderInputLine();
        }
    }
    
    private void RenderHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        
        var status = ProcessState switch
        {
            ProcessState.Running => "RUNNING",
            ProcessState.Stopped => "STOPPED",
            ProcessState.Exited => "EXITED",
            ProcessState.Error => "ERROR",
            _ => "NOT STARTED"
        };

        var headerText = $" {ProcessInfo.FileName} {ProcessInfo.Arguments} [{status}] ";
        headerText = headerText.PadRight(Width);
        
        Console.SetCursorPosition(X, Y);
        Console.Write(headerText);
        
        Console.ResetColor();
    }

    private void RenderOutput()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;

        var outputStartY = Y + 1;
        var outputHeight = Height - 2;

        for (var i = 0; i < outputHeight; i++)
        {
            var lineIndex = _scrollOffset + i;
            Console.SetCursorPosition(X, outputStartY + i);

            if (lineIndex < _outputHistory.Count)
            {
                var line = _outputHistory[lineIndex];
                if (line.Length > Width) line = line[..Width];
                
                if (line.StartsWith("[ERROR]"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(line.PadRight(Width));
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.Write(line.PadRight(Width));
                }
            }
            else
            {
                Console.Write(new string(' ', Width));
            }
        }
    }

    private void RenderInputLine()
    {
        var inputY = Y + Height - 1;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.BackgroundColor = ConsoleColor.DarkGray;
        
        const string prompt = "> ";
        var inputLine = prompt + _currentInput;
        
        if (inputLine.Length > Width)
            inputLine = inputLine[..Width];
        
        Console.SetCursorPosition(X, inputY);
        Console.Write(inputLine.PadRight(Width));
        
        if (HasFocus)
            Console.SetCursorPosition(X + prompt.Length + _currentInput.Length, inputY);
        
        Console.ResetColor();
    }

    public bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (!HasFocus) return false;

        switch (keyInfo.Key)
        {
                        case ConsoleKey.Enter:
                if (!string.IsNullOrWhiteSpace(_currentInput))
                {
                    _ = WriteInputAsync(_currentInput);
                }
                return true;

            case ConsoleKey.Backspace:
                if (_currentInput.Length > 0)
                {
                    _currentInput = _currentInput[..^1];
                    RequestRedraw?.Invoke();
                }
                return true;

            case ConsoleKey.UpArrow:
                NavigateInputHistory(-1);
                return true;

            case ConsoleKey.DownArrow:
                NavigateInputHistory(1);
                return true;

            case ConsoleKey.PageUp:
                _scrollOffset = Math.Max(0, _scrollOffset - (Height - 2));
                RequestRedraw?.Invoke();
                return true;

            case ConsoleKey.PageDown:
                _scrollOffset = Math.Min(_outputHistory.Count - (Height - 2), _scrollOffset + (Height - 2));
                RequestRedraw?.Invoke();
                return true;

            case ConsoleKey.Escape:
            case ConsoleKey.C when keyInfo.Modifiers == ConsoleModifiers.Control:
                _ = StopAsync();
                return true;
            
            default:
                if (keyInfo.KeyChar >= 32 && keyInfo.KeyChar <= 126)
                {
                    _currentInput += keyInfo.KeyChar;
                    RequestRedraw?.Invoke();
                    return true;
                }
                break;
        }
        
        return false;
    }

    private void NavigateInputHistory(int direction)
    {
        if (_inputHistory.Count == 0) return;
        
        _inputHistoryIndex = Math.Clamp(_inputHistoryIndex + direction, 0, _inputHistory.Count);

        _currentInput = _inputHistoryIndex == _inputHistory.Count ? string.Empty : _inputHistory[_inputHistoryIndex];
        
        RequestRedraw?.Invoke();
    }
    
    public Task OnDestroyAsync()
    {
        State = ComponentState.Destroyed;
        return StopAsync();
    }
    
    public Task OnResizeAsync(int width, int height)
    {
        Width = width;
        Height = height;
        RequestRedraw?.Invoke();
        return Task.CompletedTask;
    }
}