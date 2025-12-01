using System.Diagnostics;
using System.Text;
using WaffleCLI.Abstractions.TUI.Processes;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Processes
{
    /// <summary>
    /// Cross-platform process runner implementation
    /// </summary>
    public class ProcessRunner : IProcessRunner
    {
        private Process _process;
        private readonly StringBuilder _outputBuffer = new StringBuilder();
        private readonly object _lock = new object();
        private bool _disposed;
        

        public int ProcessId => _process?.Id ?? -1;
        public bool IsRunning => _process != null && !_process.HasExited;
        public int? ExitCode => _process?.HasExited == true ? _process.ExitCode : (int?)null;
        public string CommandLine { get; private set; }

        public event EventHandler<ProcessOutputEventArgs> OutputReceived;
        public event EventHandler<ProcessExitedEventArgs> Exited;

        public async Task StartAsync(
            string fileName,
            string arguments = "",
            string workingDirectory = null,
            Dictionary<string, string> environmentVariables = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            // Create process start info
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            // Set environment variables
            if (environmentVariables != null)
            {
                foreach (var kvp in environmentVariables)
                {
                    startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }
            }

            CommandLine = $"{fileName} {arguments}".Trim();

            TuiLogger.LogInfo($"Starting process: {CommandLine}");

            try
            {
                _process = new Process { StartInfo = startInfo };
                _process.EnableRaisingEvents = true;

                // Setup output handlers
                _process.OutputDataReceived += OnOutputDataReceived;
                _process.ErrorDataReceived += OnErrorDataReceived;
                _process.Exited += OnProcessExited;

                // Start process
                if (!_process.Start())
                {
                    throw new InvalidOperationException($"Failed to start process: {CommandLine}");
                }

                // Begin async output reading
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                TuiLogger.LogInfo($"Process started with PID: {_process.Id}");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to start process: {CommandLine}", ex);
                throw new InvalidOperationException($"Failed to start process: {ex.Message}", ex);
            }

            await Task.CompletedTask;
        }

        public async Task SendInputAsync(string input)
        {
            if (!IsRunning || _process == null)
                throw new InvalidOperationException("Process is not running");

            try
            {
                await _process.StandardInput.WriteAsync(input);
                TuiLogger.LogDebug($"Sent input to process {ProcessId}: {input}");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to send input to process {ProcessId}", ex);
                throw;
            }
        }

        public async Task SendLineAsync(string input)
        {
            if (!IsRunning || _process == null)
                throw new InvalidOperationException("Process is not running");

            try
            {
                await _process.StandardInput.WriteLineAsync(input);
                TuiLogger.LogDebug($"Sent line to process {ProcessId}: {input}");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to send line to process {ProcessId}", ex);
                throw;
            }
        }

        public void Kill()
        {
            if (!IsRunning || _process == null) return;

            try
            {
                TuiLogger.LogInfo($"Killing process {ProcessId}");
                _process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to kill process {ProcessId}", ex);
            }
        }

        public async Task<bool> WaitForExitAsync(int timeout = -1)
        {
            if (_process == null) return true;

            try
            {
                if (timeout > 0)
                {
                    return await Task.Run(() => _process.WaitForExit(timeout));
                }
                else
                {
                    await Task.Run(() => _process.WaitForExit());
                    return true;
                }
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Error waiting for process exit {ProcessId}", ex);
                return false;
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                TuiLogger.LogInfo($"Process {ProcessId} output: {e.Data}");
                OutputReceived?.Invoke(this, new ProcessOutputEventArgs(e.Data));
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                TuiLogger.LogWarning($"Process {ProcessId} error: {e.Data}");
                OutputReceived?.Invoke(this, new ProcessOutputEventArgs(e.Data, isError: true));
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            TuiLogger.LogInfo($"Process {ProcessId} exited with code: {_process?.ExitCode ?? -1}");
            Exited?.Invoke(this, new ProcessExitedEventArgs(_process?.ExitCode ?? -1));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_process != null)
                    {
                        try
                        {
                            // Stop reading async
                            _process.CancelOutputRead();
                            _process.CancelErrorRead();

                            // Remove event handlers
                            _process.OutputDataReceived -= OnOutputDataReceived;
                            _process.ErrorDataReceived -= OnErrorDataReceived;
                            _process.Exited -= OnProcessExited;

                            // Kill if still running
                            if (IsRunning)
                            {
                                _process.Kill();
                            }

                            _process.Dispose();
                        }
                        catch (Exception ex)
                        {
                            TuiLogger.LogError($"Error disposing process runner for PID {ProcessId}", ex);
                        }
                        finally
                        {
                            _process = null;
                        }
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}