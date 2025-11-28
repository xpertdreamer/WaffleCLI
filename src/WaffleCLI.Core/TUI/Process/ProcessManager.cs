using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Events;
using WaffleCLI.Core.TUI.Process.Events;

namespace WaffleCLI.Core.TUI.Process;

public class ProcessManager
{
    private readonly List<IProcessComponent> _runningProcesses = [];
    private readonly TuiEventBus _eventBus;

    public ProcessManager(TuiEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public IReadOnlyList<IProcessComponent> RunningProcesses => _runningProcesses.AsReadOnly();

    public IProcessComponent CreateProcess(ProcessInfo processInfo)
    {
        var processComponent = new ProcessRunnerComponent(processInfo);

        processComponent.ProcessExited += exitCode =>
        {
            _eventBus.Publish(new ProcessExitedEvent(processComponent, exitCode));
        };
        
        processComponent.OutputReceived += output =>
        {
            _eventBus.Publish(new ProcessOutputEvent(processComponent, output));
        };
        
        _runningProcesses.Add(processComponent);
        _eventBus.Publish(new ProcessCreatedEvent(processComponent));
        
        return processComponent;
    }
    
    public async Task StopAllProcessesAsync()
    {
        var stopTasks = _runningProcesses.Select(p => p.StopAsync());
        await Task.WhenAll(stopTasks);
        _runningProcesses.Clear();
    }

    public void RemoveProcess(IProcessComponent process)
    {
        _runningProcesses.Remove(process);
    }
}