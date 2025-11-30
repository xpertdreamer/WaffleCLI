namespace WaffleCLI.Core.TUI.Infrastructure.Logging
{
    /// <summary>
    /// Enhanced logger for TUI framework with quiet mode for production
    /// </summary>
    public static class TuiLogger
    {
        public static bool EnableLogging { get; set; } = true;
        public static bool QuietMode { get; set; } = false; // When true, doesn't output to console
        public static string LogFile { get; set; } = "tui.log";
        private static readonly object _lockObject = new object();
        private static int _logSequence = 0;
        private static DateTime _lastLogTime = DateTime.MinValue;

        public static void Log(string message, bool forceConsoleOutput = false)
        {
            if (!EnableLogging) return;

            lock (_lockObject)
            {
                try
                {
                    _logSequence++;
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logMessage = $"[{_logSequence:000000}] {timestamp} - {message}";
                    
                    // Only output to console in debug mode or when forced
                    if (!QuietMode || forceConsoleOutput)
                    {
                        // Only log to console if it's been more than 1 second since last console log
                        // to avoid spamming the console during rendering
                        if (forceConsoleOutput || (DateTime.Now - _lastLogTime).TotalSeconds >= 1.0)
                        {
                            Console.WriteLine($"[LOG] {message}");
                            _lastLogTime = DateTime.Now;
                        }
                    }
                    
                    // Always write to file
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
            var errorMessage = $"❌ ERROR: {message}";
            if (ex != null)
            {
                errorMessage += $"\n       Exception: {ex.GetType().Name}: {ex.Message}";
                errorMessage += $"\n       Stack Trace: {ex.StackTrace}";
            }
            Log(errorMessage, true); // Force console output for errors
        }

        public static void LogInfo(string message)
        {
            Log($"ℹ️ INFO: {message}");
        }

        public static void LogDebug(string message)
        {
            // Debug logs are less important - don't force console output
            Log($"🔍 DEBUG: {message}");
        }

        public static void LogWarning(string message)
        {
            Log($"⚠️ WARN: {message}");
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
                    Log("Log file cleared - new session started", true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}