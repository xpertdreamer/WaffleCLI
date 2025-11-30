namespace WaffleCLI.Abstractions.TUI.Exceptions
{
    /// <summary>
    /// Base exception for TUI framework
    /// </summary>
    public class TuiException : Exception
    {
        public TuiException() { }
        public TuiException(string message) : base(message) { }
        public TuiException(string message, Exception inner) : base(message, inner) { }
    }
}