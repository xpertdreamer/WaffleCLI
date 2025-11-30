using System.Data;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Elements;
using WaffleCLI.Core.TUI.Process;

namespace WaffleCLI.Core.TUI.Screens;

public class ProcessManagerScreen : BasicTuiScreen
{
    private readonly ProcessManager _processManager;
    private readonly List<IProcessComponent> _processes = [];
    private IProcessComponent _activeProcess;
    private TextElement _statusElement;

    public ProcessManagerScreen(ProcessManager processManager)
    {
        _processManager = processManager;
    }
    
    public override string Title => "Process Manager";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        ClearElements();

        _statusElement = new TextElement
        {   
            X = 2, Y = 2, Width = 76,
            Text = "Ready - Press F1 for CMD, F2 for Bash, F3 for Custom, F4 to Kill All",
            Color = ConsoleColor.Yellow,
            isFocusable = false
        };

        var newCmdButton = new ButtonElement
        {
            X = 2, Y = 4, Width = 15,
            Text = "New CMD (F1)",
            isFocusable = true
        };

        var newBashButton = new ButtonElement
        {
            X = 20, Y = 4, Width = 15,
            Text = "New Bash (F2)",
            isFocusable = true
        };
        
        var customButton = new ButtonElement
        {
            X = 38, Y = 4, Width = 15,
            Text = "Custom (F3)",
            isFocusable = true
        };

        var killAllButton = new ButtonElement
        {
            X = 56, Y = 4, Width = 15,
            Text = "Kill All (F4)",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.Red,
            isFocusable = true
        };

        newCmdButton.Clicked += () => CreateNewProcess("cmd");
        newBashButton.Clicked += () => CreateNewProcess("bash");
        customButton.Clicked += () => ShowCustomProcessDialog();
        killAllButton.Clicked += () => _ = KillAllProcesses();

        AddElement(_statusElement);
        AddElement(newCmdButton);
        AddElement(newBashButton);
        AddElement(customButton);
        AddElement(killAllButton);

        CreateNewProcess(Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd" : "bash");

        UpdateStatus();
    }
    
     protected override void RecalculateLayout()
    {
        var screenWidth = Console.WindowWidth;
        var screenHeight = Console.WindowHeight;

        _statusElement.Width = Math.Max(10, screenWidth - 4);
        _statusElement.X = 2;

        var processY = 6;
        foreach (var process in _processes)
        {
            process.X = 2;
            process.Y = processY;
            process.Width = Math.Max(10, screenWidth - 4);
            process.Height = Math.Max(5, screenHeight - processY - 2);
            
            processY += process.Height + 1;
            
            if (processY >= screenHeight - 5) break;
        }

        UpdateStatus();
    }

    private void CreateNewProcess(string type)
    {
        try
        {
            var process = ProcessComponentFactory.CreateInteractiveShell();
            switch (type)
            {
                case "cmd":
                {
                    if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                    {
                        UpdateStatus("CMD only available on Windows. Using Bash instead.");
                        process = ProcessComponentFactory.CreateInteractiveShell();
                    }

                    break;
                }
                case "bash":
                    break;
            }

            ConfigureProcessComponent(process);
            _processes.Add(process);
            AddElement(process);
            
            _activeProcess = process;
            SetFocus(process);
            
            _ = process.StartAsync();
            
            UpdateStatus($"Started {type} process. Total processes: {_processes.Count}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error creating process: {ex.Message}");
        }
    }

    private void ConfigureProcessComponent(IProcessComponent process)
    {
        process.ProcessExited += exitCode =>
        {
            _processes.Remove(process);
            RemoveElement(process);
            
            if (_activeProcess == process)
            {
                _activeProcess = _processes.FirstOrDefault();
                if (_activeProcess != null)
                    SetFocus(_activeProcess);
            }
            
            UpdateStatus($"Process exited with code {exitCode}. Remaining: {_processes.Count}");
        };

        process.OutputReceived += output =>
        {
            
        };

        process.ErrorReceived += error =>
        {
            UpdateStatus($"Process error: {error}");
        };
    }

    private void ShowCustomProcessDialog()
    {
        var dialog = new TextElement
        {
            X = 10, Y = 10, Width = 60, Height = 5,
            Text = "Enter command (e.g., python, node, dotnet):",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkBlue,
            HasBorder = true,
            isFocusable = false
        };

        AddElement(dialog);

        CreateNewProcess("custom");
        
        Task.Delay(2000).ContinueWith(_ => 
        {
            RemoveElement(dialog);
            RequestRedraw();
        });
    }

    private async Task KillAllProcesses()
    {
        if (_processes.Count == 0)
        {
            UpdateStatus("No processes to kill");
            return;
        }

        var processesToKill = _processes.ToList();
        foreach (var process in processesToKill)
        {
            await process.StopAsync();
        }

        _processes.Clear();
        UpdateStatus($"Killed {processesToKill.Count} processes");
    }

    private void UpdateStatus(string message = null)
    {
        if (_statusElement != null)
        {
            if (!string.IsNullOrEmpty(message))
            {
                _statusElement.Text = message;
            }
            else
            {
                _statusElement.Text = $"Processes: {_processes.Count} | " +
                                    $"Active: {(_activeProcess != null ? "Yes" : "No")} | " +
                                    $"Screen: {Console.WindowWidth}x{Console.WindowHeight}";
            }
        }
        RequestRedraw();
    }

    public override Task HandleInputAsync(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.F1:
                CreateNewProcess("cmd");
                return Task.CompletedTask;

            case ConsoleKey.F2:
                CreateNewProcess("bash");
                return Task.CompletedTask;

            case ConsoleKey.F3:
                ShowCustomProcessDialog();
                return Task.CompletedTask;

            case ConsoleKey.F4:
                _ = KillAllProcesses();
                return Task.CompletedTask;

            case ConsoleKey.Tab when keyInfo.Modifiers == ConsoleModifiers.Control:
                SwitchToNextProcess();
                return Task.CompletedTask;

            case ConsoleKey.C when keyInfo.Modifiers == ConsoleModifiers.Control:
                if (_activeProcess != null)
                {
                    _ = _activeProcess.StopAsync();
                }
                return Task.CompletedTask;
        }

        return base.HandleInputAsync(keyInfo);
    }

    private void SwitchToNextProcess()
    {
        if (_processes.Count < 2) return;

        var currentIndex = _processes.IndexOf(_activeProcess);
        var nextIndex = (currentIndex + 1) % _processes.Count;
        
        _activeProcess = _processes[nextIndex];
        SetFocus(_activeProcess);
        UpdateStatus($"Switched to process {nextIndex + 1} of {_processes.Count}");
    }

    public override async Task HandleResizeAsync()
    {
        await base.HandleResizeAsync();
        UpdateStatus($"Screen resized to {Console.WindowWidth}x{Console.WindowHeight}");
    }

    private static void RequestRedraw()
    {
        
    }
}

