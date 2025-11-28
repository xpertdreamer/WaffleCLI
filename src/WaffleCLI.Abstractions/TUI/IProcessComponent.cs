namespace WaffleCLI.Abstractions.TUI;

public interface IProcessComponent : ITuiComponent
{
    ProcessInfo ProcessInfo { get; }
    ProcessState State { get; }
    string Output { get; }
    string Error { get; }

    Task StartAsync();
    Task StopAsync();
    Task WriteInputAsync(string input);
    event Action<string> OutputReceived;
    event Action<string> ErrorReceived;
    event Action<int> ProcessExited;
}

public enum ProcessState
{
    NotStarted,
    Running,
    Stopped,
    Exited,
    Error
}

public record ProcessInfo(
    string? FileName,
    string Arguments = "",
    string? WorkingDirectory = "",
    Dictionary<string, string> EnviromentVariables = null,
    bool UseShellExecute = false
);