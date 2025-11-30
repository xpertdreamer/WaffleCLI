namespace WaffleCLI.Abstractions.TUI.Components
{
    /// <summary>
    /// Container component that can hold other components
    /// </summary>
    public interface IContainer : IComponent
    {
        void DoLayout();
    }
}