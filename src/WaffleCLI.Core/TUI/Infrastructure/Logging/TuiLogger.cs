namespace WaffleCLI.Core.TUI.Infrastructure.Logging
{
    /// <summary>
    /// Enhanced logger for TUI framework with asynchronous file writing
    /// </summary>
    public static class TuiLogger
    {
        private static readonly object _initLock = new object();
        private static readonly object _consoleLock = new object();
        private static bool _isInitialized = false;
        private static System.Threading.Channels.Channel<string> _logChannel;
        private static Task _writeTask;
        private static CancellationTokenSource _cancellationTokenSource;
        private static readonly List<string> _logBuffer = new List<string>();
        private static readonly System.Diagnostics.Stopwatch _bufferTimer = new System.Diagnostics.Stopwatch();
        private const int BUFFER_FLUSH_INTERVAL_MS = 100;
        private const int MAX_BUFFER_SIZE = 50;
        
        public static bool EnableLogging { get; set; } = true;
        public static bool QuietMode { get; set; } = false; // When true, doesn't output to console
        public static string LogFile { get; set; } = "tui.log";
        private static int _logSequence = 0;
        private static DateTime _lastLogTime = DateTime.MinValue;

        /// <summary>
        /// Initializes the async logging system
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                lock (_initLock)
                {
                    if (!_isInitialized)
                    {
                        _cancellationTokenSource = new CancellationTokenSource();
                        _logChannel = System.Threading.Channels.Channel.CreateUnbounded<string>(
                            new System.Threading.Channels.UnboundedChannelOptions 
                            { 
                                SingleReader = true, 
                                SingleWriter = false 
                            });
                        
                        _writeTask = Task.Run(async () => await ProcessLogQueueAsync(_cancellationTokenSource.Token));
                        _bufferTimer.Start();
                        
                        _isInitialized = true;
                        
                        // Write initial log message synchronously
                        string initMessage = $"[000000] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Logging system initialized";
                        try
                        {
                            File.AppendAllText(LogFile, initMessage + Environment.NewLine);
                        }
                        catch
                        {
                            // Ignore initialization errors
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the log queue asynchronously
        /// </summary>
        private static async Task ProcessLogQueueAsync(CancellationToken cancellationToken)
        {
            var reader = _logChannel.Reader;
            var flushTimer = new System.Threading.Timer(FlushBuffer, null, 
                BUFFER_FLUSH_INTERVAL_MS, BUFFER_FLUSH_INTERVAL_MS);
            
            try
            {
                await foreach (var logEntry in reader.ReadAllAsync(cancellationToken))
                {
                    lock (_logBuffer)
                    {
                        _logBuffer.Add(logEntry);
                        
                        // Flush if buffer is getting large
                        if (_logBuffer.Count >= MAX_BUFFER_SIZE)
                        {
                            FlushBufferInternal();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            finally
            {
                flushTimer.Dispose();
                
                // Final flush on shutdown
                lock (_logBuffer)
                {
                    if (_logBuffer.Count > 0)
                    {
                        FlushBufferInternal();
                    }
                }
            }
        }

        /// <summary>
        /// Timer callback to flush buffer periodically
        /// </summary>
        private static void FlushBuffer(object state)
        {
            lock (_logBuffer)
            {
                if (_logBuffer.Count > 0 && _bufferTimer.ElapsedMilliseconds >= BUFFER_FLUSH_INTERVAL_MS)
                {
                    FlushBufferInternal();
                    _bufferTimer.Restart();
                }
            }
        }

        /// <summary>
        /// Writes buffered logs to file
        /// </summary>
        private static void FlushBufferInternal()
        {
            if (_logBuffer.Count == 0)
                return;
                
            try
            {
                File.AppendAllLines(LogFile, _logBuffer);
                _logBuffer.Clear();
            }
            catch (Exception ex)
            {
                // Last resort - output to debug output
                System.Diagnostics.Debug.WriteLine($"LOGGER FAILED TO WRITE: {ex.Message}");
                _logBuffer.Clear(); // Clear buffer even on error to prevent infinite growth
            }
        }

        /// <summary>
        /// Main logging method
        /// </summary>
        public static void Log(string message, bool forceConsoleOutput = false)
        {
            if (!EnableLogging) return;

            EnsureInitialized();
            
            lock (_consoleLock)
            {
                try
                {
                    _logSequence++;
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logMessage = $"[{_logSequence:000000}] {timestamp} - {message}";
                    
                    // Only output to console in debug mode or when forced
                    if (!QuietMode || forceConsoleOutput)
                    {
                        // Throttle console output to avoid overwhelming the console
                        if (forceConsoleOutput || (DateTime.Now - _lastLogTime).TotalSeconds >= 1.0)
                        {
                            Console.WriteLine($"[LOG] {message}");
                            _lastLogTime = DateTime.Now;
                        }
                    }
                    
                    // Write to async channel (non-blocking)
                    if (_logChannel != null)
                    {
                        _logChannel.Writer.TryWrite(logMessage);
                    }
                    else
                    {
                        // Fallback synchronous write if channel not available
                        lock (_logBuffer)
                        {
                            _logBuffer.Add(logMessage);
                            if (_logBuffer.Count >= 5) // Small threshold for fallback
                            {
                                FlushBufferInternal();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Last resort - output to debug output
                    System.Diagnostics.Debug.WriteLine($"LOGGER FAILED: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Shuts down the logging system
        /// </summary>
        public static void Shutdown()
        {
            if (_isInitialized)
            {
                lock (_initLock)
                {
                    if (_isInitialized)
                    {
                        _cancellationTokenSource?.Cancel();
                        _writeTask?.Wait(TimeSpan.FromSeconds(2));
                        
                        _cancellationTokenSource?.Dispose();
                        _logChannel?.Writer.Complete();
                        
                        // Final flush
                        lock (_logBuffer)
                        {
                            if (_logBuffer.Count > 0)
                            {
                                FlushBufferInternal();
                            }
                        }
                        
                        _isInitialized = false;
                    }
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
            lock (_consoleLock)
            {
                lock (_initLock)
                {
                    try
                    {
                        // Shutdown existing logging system
                        Shutdown();
                        
                        // Clear the file
                        if (File.Exists(LogFile))
                        {
                            File.Delete(LogFile);
                        }
                        
                        // Reset state
                        _logSequence = 0;
                        _logBuffer.Clear();
                        _isInitialized = false;
                        
                        // Re-initialize
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
}