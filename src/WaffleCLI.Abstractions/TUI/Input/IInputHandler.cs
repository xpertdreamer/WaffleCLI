namespace WaffleCLI.Abstractions.TUI.Input
{
    /// <summary>
    /// Input handler interface
    /// </summary>
    public interface IInputHandler
    {
        void ProcessInput();
        void Stop();
    }
}