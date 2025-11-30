namespace WaffleCLI.Abstractions.TUI.Components.Interfaces
{
    /// <summary>
    /// Text box component interface
    /// </summary>
    public interface ITextBox : IFocusable
    {
        string Text { get; set; }
        string Placeholder { get; set; }
        int MaxLength { get; set; }
    }
}