using WaffleCLI.Core.TUI;

namespace WaffleCLI.Runtime.TUI.Elements;

/// <summary>
/// Represents a scrollable list view element for Text User Interfaces with selection capabilities.
/// </summary>
/// <remarks>
/// This element displays a list of items with support for keyboard navigation, selection highlighting,
/// and automatic scrolling. It can be used for menus, option lists, or any scenario requiring
/// item selection from a list.
/// </remarks>
public class TuiListView : TuiElement
{
    private readonly List<string> _items = [];
    private int _selectedIndex = 0;
    private int _scrollOffset = 0;

    /// <summary>
    /// Occurs when an item is selected, either by pressing Enter or navigating to it.
    /// </summary>
    /// <remarks>
    /// The event provides the index of the selected item. This event fires both when
    /// navigating between items and when explicitly selecting with Enter.
    /// </remarks>
    public event Action<int>? ItemSelected;

    /// <summary>
    /// Gets the list of items displayed in the list view.
    /// </summary>
    public List<string> Items => _items;

    /// <summary>
    /// Gets the index of the currently selected item.
    /// </summary>
    public int SelectedIndex => _selectedIndex;

    /// <summary>
    /// Sets the items to be displayed in the list view and adjusts selection state.
    /// </summary>
    /// <param name="items">The collection of items to display.</param>
    /// <remarks>
    /// This method clears the current items and replaces them with the new collection.
    /// It automatically adjusts the selected index and scroll offset to remain valid
    /// with the new item set.
    /// </remarks>
    public void SetItems(IEnumerable<string> items)
    {
        _items.Clear();
        _items.AddRange(items);
        _selectedIndex = Math.Min(_selectedIndex, _items.Count - 1);
        _scrollOffset = Math.Min(_scrollOffset, _items.Count - 1);
    }

    /// <summary>
    /// Renders the list view to the console with proper formatting and selection highlighting.
    /// </summary>
    /// <remarks>
    /// Draws a bordered box containing the list items. The selected item is highlighted
    /// with different colors. Long items are truncated with an ellipsis to fit within
    /// the available width. The view automatically handles scrolling to show the
    /// currently selected item.
    /// </remarks>
    public override void Render()
    {
        var originalBackgroundColor = Console.BackgroundColor;
        var originalTextColor = Console.ForegroundColor;
        
        DrawBox(0, 0, Width, Height);
        
        int itemsToShow = Math.Min(Height - 2, _items.Count - _scrollOffset);

        for (int i = 0; i < itemsToShow; i++)
        {
            var itemsIndex = _scrollOffset + i;
            SetCursorPosition(1, i + 1);

            if (itemsIndex == _selectedIndex)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.BackgroundColor = BackgroundColor;
                Console.ForegroundColor = ForegroundColor;
            }
            
            var item = _items[itemsIndex];
            var displayText = item.Length > Width - 2 ? item[..(Width - 5)] + "..." : item.PadRight(Width - 2);
            Console.Write(displayText);
        }
        
        Console.BackgroundColor = originalBackgroundColor;
        Console.ForegroundColor = originalTextColor;
    }

    /// <summary>
    /// Handles keyboard input for list navigation and selection.
    /// </summary>
    /// <param name="keyInfo">The console key information containing the pressed key and modifiers.</param>
    /// <returns>True if the key was handled by this list view; otherwise, false.</returns>
    /// <remarks>
    /// Handles UpArrow and DownArrow for navigation, and Enter for selection.
    /// Automatically adjusts scroll offset to keep the selected item visible.
    /// Fires the ItemSelected event when navigation changes or Enter is pressed.
    /// </remarks>
    public override bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                if (_selectedIndex > 0)
                {
                    _selectedIndex--;
                    if (_selectedIndex < _scrollOffset)
                        _scrollOffset = _selectedIndex;
                    ItemSelected?.Invoke(_selectedIndex);
                    return true;
                }
                break;
            
            case ConsoleKey.DownArrow:
                if (_selectedIndex < _items.Count - 1)
                {
                    _selectedIndex++;
                    if (_selectedIndex >= _scrollOffset + (Height - 2))
                        _scrollOffset = _selectedIndex - (Height - 2) + 1;
                    ItemSelected?.Invoke(_selectedIndex);
                    return true;
                }
                break;
            
            case ConsoleKey.Enter:
                ItemSelected?.Invoke(_selectedIndex);
                return true;
        }
        
        return false;
    }
}