using WaffleCLI.Abstractions.TUI.Input;

namespace WaffleCLI.Abstractions.TUI.Components
{
    /// <summary>
    /// Component that can receive focus
    /// </summary>
    public interface IFocusable : IComponent
    {
        bool HasFocus { get; set; }
        void OnFocus();
        void OnBlur();
        bool HandleInput(InputEvent inputEvent);
    }
}