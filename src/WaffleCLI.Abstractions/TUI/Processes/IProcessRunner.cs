namespace WaffleCLI.Abstractions.TUI.Processes
{
    /// <summary>
    /// Event arguments for process output
    /// </summary>
    public class ProcessOutputEventArgs : EventArgs
    {
        public string Output { get; }
        public bool IsError { get; }
        public DateTime Timestamp { get; }

        public ProcessOutputEventArgs(string output, bool isError = false)
        {
            Output = output;
            IsError = isError;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Event arguments for process exit
    /// </summary>
    public class ProcessExitedEventArgs : EventArgs
    {
        public int ExitCode { get; }
        public DateTime ExitTime { get; }

        public ProcessExitedEventArgs(int exitCode)
        {
            ExitCode = exitCode;
            ExitTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Interface for running external processes
    /// </summary>
    public interface IProcessRunner : IDisposable
    {
        /// <summary>
        /// Gets the process ID
        /// </summary>
        int ProcessId { get; }

        /// <summary>
        /// Gets whether the process is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the exit code (if process has exited)
        /// </summary>
        int? ExitCode { get; }

        /// <summary>
        /// Gets the command line used to start the process
        /// </summary>
        string CommandLine { get; }

        /// <summary>
        /// Event raised when process outputs data
        /// </summary>
        event EventHandler<ProcessOutputEventArgs> OutputReceived;

        /// <summary>
        /// Event raised when process exits
        /// </summary>
        event EventHandler<ProcessExitedEventArgs> Exited;

        /// <summary>
        /// Starts a process
        /// </summary>
        /// <param name="fileName">Executable file name</param>
        /// <param name="arguments">Command line arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <param name="environmentVariables">Environment variables</param>
        /// <returns>Task representing the start operation</returns>
        Task StartAsync(
            string fileName,
            string arguments = "",
            string workingDirectory = null,
            Dictionary<string, string> environmentVariables = null);

        /// <summary>
        /// Sends input to the process
        /// </summary>
        /// <param name="input">Input string</param>
        Task SendInputAsync(string input);

        /// <summary>
        /// Sends a line of input to the process
        /// </summary>
        /// <param name="input">Input line</param>
        Task SendLineAsync(string input);

        /// <summary>
        /// Kills the process
        /// </summary>
        void Kill();

        /// <summary>
        /// Waits for the process to exit
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if process exited, false if timeout</returns>
        Task<bool> WaitForExitAsync(int timeout = -1);
    }
}