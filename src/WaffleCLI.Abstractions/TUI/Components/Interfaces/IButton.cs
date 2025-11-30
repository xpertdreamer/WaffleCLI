namespace WaffleCLI.Abstractions.TUI.Components.Interfaces
{
    /// <summary>
    /// Button component interface
    /// </summary>
    public interface IButton : IFocusable
    {
        string Text { get; set; }
        Action? OnClick { get; set; }
    }
}