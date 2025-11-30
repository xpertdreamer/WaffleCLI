using System.Collections;

namespace WaffleCLI.Abstractions.TUI.Components.Interfaces
{
    /// <summary>
    /// List box component interface
    /// </summary>
    public interface IListBox : IFocusable
    {
        IList Items { get; set; } // Changed to non-generic IList
        int SelectedIndex { get; set; }
        string? SelectedItem { get; }
        Action<int>? OnSelectionChanged { get; set; }
    }
}