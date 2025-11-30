namespace WaffleCLI.Core.TUI.Infrastructure.Logging
{
    /// <summary>
    /// Enhanced logger for TUI framework with detailed debugging
    /// </summary>
    public static class TuiLogger
    {
        public static bool EnableLogging { get; set; } = true;
        public static string LogFile { get; set; } = "tui.log";
        private static readonly object _lockObject = new object();
        private static int _logSequence = 0;

        public static void Log(string message)
        {
            if (!EnableLogging) return;

            lock (_lockObject)
            {
                try
                {
                    _logSequence++;
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logMessage = $"[{_logSequence:000000}] {timestamp} - {message}";
                    
                    Console.WriteLine($"[LOG] {message}"); // Also output to console for immediate feedback
                    File.AppendAllText(LogFile, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Last resort - output to debug output
                    System.Diagnostics.Debug.WriteLine($"LOGGER FAILED: {ex.Message}");
                }
            }
        }

        public static void LogError(string message, Exception? ex = null)
        {
            var errorMessage = $"ERROR: {message}";
            if (ex != null)
            {
                errorMessage += $"\n       Exception: {ex.GetType().Name}: {ex.Message}";
                errorMessage += $"\n       Stack Trace: {ex.StackTrace}";
            }
            Log(errorMessage);
        }

        public static void LogInfo(string message)
        {
            Log($" INFO: {message}");
        }

        public static void LogDebug(string message)
        {
            Log($" DEBUG: {message}");
        }

        public static void LogWarning(string message)
        {
            Log($" WARN: {message}");
        }

        public static void ClearLog()
        {
            lock (_lockObject)
            {
                try
                {
                    if (File.Exists(LogFile))
                    {
                        File.Delete(LogFile);
                    }
                    _logSequence = 0;
                    Log("Log file cleared - new session started");
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}