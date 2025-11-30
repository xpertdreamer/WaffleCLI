using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Abstractions.TUI.Components.Interfaces
{
    /// <summary>
    /// Label component interface
    /// </summary>
    public interface ILabel : IComponent
    {
        string Text { get; set; }
        TextAlignment TextAlignment { get; set; }
    }
}