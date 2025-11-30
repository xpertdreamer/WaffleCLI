using System.Diagnostics;
using System.Text;

namespace WaffleCLI.Core.TUI.Diagnostics;

public class TuiDiagnostics
{
    private readonly StringBuilder _log = new();
    private readonly string _logFile;
    private readonly object _lock = new();

    public TuiDiagnostics()
    {
        _logFile = Path.Combine(Environment.CurrentDirectory, $"tui_diagnostics_{DateTime.Now:yyyyMMdd_HHmmss}.log");
    }

    public void Log(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] [{caller}] {message}";
            _log.AppendLine(logMessage);
            
            Debug.WriteLine(logMessage);
            
            File.AppendAllText(_logFile, logMessage + Environment.NewLine);
        }
    }

    public void LogState(string context, object state)
    {
        try
        {
            var stateJson = System.Text.Json.JsonSerializer.Serialize(state, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true,
                MaxDepth = 3
            });
            Log($"{context}: {stateJson}");
        }
        catch (Exception ex)
        {
            Log($"{context}: Failed to serialize state - {ex.Message}");
        }
    }

    public string GetLog() => _log.ToString();
    public void Clear() => _log.Clear();
}

public static class TuiDiagnosticsService
{
    private static TuiDiagnostics _instance = new TuiDiagnostics();
    public static TuiDiagnostics Instance => _instance;
    
    public static void Reset() => _instance = new TuiDiagnostics();
}