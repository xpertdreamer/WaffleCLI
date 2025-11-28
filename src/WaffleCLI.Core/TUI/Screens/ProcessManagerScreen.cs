using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Elements;
using WaffleCLI.Core.TUI.Process;

namespace WaffleCLI.Core.TUI.Screens;

public class ProcessManagerScreen : BasicTuiScreen
{
    private readonly ProcessManager _processManager;
    private readonly List<ITuiElement> _processViews = [];
    private IProcessComponent _activeProcess;

    public ProcessManagerScreen(ProcessManager processManager)
    {
        _processManager = processManager;
    }
    
    public override string Title => "Process Manager";

    public override Task InitializeAsync()
    {
        ClearElements();

        var controls = new TextElement
        {
            X = 2, Y = 2, Width = 76,
            Text = "F1: New CMD | F2: New Bash | F3: New Process | F4: Kill All | Tab: Switch Process",
            Color = ConsoleColor.Yellow,
            isFocusable = false
        };

        var newCmdButton = new ButtonElement
        {
            X = 2, Y = 4, Width = 15,
            Text = "New CMD",
            isFocusable = true
        };

        var newBashButton = new ButtonElement
        {
            X = 20, Y = 4, Width = 15,
            Text = "New Bash",
            isFocusable = true
        };

        var killAllButton = new ButtonElement
        {
            X = 60, Y = 4, Width = 15,
            Text = "Kill All",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.Red,
            isFocusable = true
        };

        newCmdButton.Clicked += () => CreateNewProcess("cmd");
        newBashButton.Clicked += () => CreateNewProcess("bash");
        killAllButton.Clicked += () => _ = _processManager.StopAllProcessesAsync();

        AddElement(controls);
        AddElement(newCmdButton);
        AddElement(newBashButton);
        AddElement(killAllButton);

        CreateNewProcess(Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd" : "bash");

        return Task.CompletedTask;
    }
    
    private void CreateNewProcess(string type)
    {
        var process = type.ToLower() switch
        {
            _ => ProcessComponentFactory.CreateInteractiveShell()
        };

        process.X = 2;
        process.Y = 6;
        process.Width = 76;
        process.Height = 16;
        process.isFocusable = true;

        process.ProcessExited += exitCode =>
        {
            _processViews.Remove(process);
            RemoveElement(process);
            
            if (_activeProcess == process)
            {
                _activeProcess = _processViews.OfType<IProcessComponent>().FirstOrDefault();
                SetFocus(_activeProcess);
            }
        };

        _processManager.CreateProcess(process.ProcessInfo);
        _processViews.Add(process);
        AddElement(process);
        
        _activeProcess = process;
        SetFocus(process);
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
                CreateNewProcess("custom");
                return Task.CompletedTask;

            case ConsoleKey.F4:
                _ = _processManager.StopAllProcessesAsync();
                return Task.CompletedTask;

            case ConsoleKey.Tab when keyInfo.Modifiers == ConsoleModifiers.Control:
                SwitchToNextProcess();
                return Task.CompletedTask;
        }

        return base.HandleInputAsync(keyInfo);
    }

    private void SwitchToNextProcess()
    {
        var processes = _processViews.OfType<IProcessComponent>().ToList();
        if (processes.Count < 2) return;

        var currentIndex = processes.IndexOf(_activeProcess);
        var nextIndex = (currentIndex + 1) % processes.Count;
        
        _activeProcess = processes[nextIndex];
        SetFocus(_activeProcess);
    }
}

