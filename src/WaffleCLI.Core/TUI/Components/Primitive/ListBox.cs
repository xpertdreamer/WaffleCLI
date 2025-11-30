using System.Collections;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// List box component
    /// </summary>
    public class ListBox : FocusableComponentBase, IListBox
    {
        private readonly ArrayList _items = new();
        private int _selectedIndex = -1;
        private int _scrollOffset = 0;

        public IList Items 
        { 
            get => _items; 
            set
            {
                _items.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        _items.Add(item);
                    }
                }
                EnsureVisible(_selectedIndex);
            }
        }

        public int SelectedIndex 
        { 
            get => _selectedIndex;
            set
            {
                var newIndex = Math.Clamp(value, -1, _items.Count - 1);
                if (_selectedIndex != newIndex)
                {
                    _selectedIndex = newIndex;
                    EnsureVisible(_selectedIndex);
                    OnSelectionChanged?.Invoke(_selectedIndex);
                }
            }
        }

        public string? SelectedItem => _selectedIndex >= 0 ? _items[_selectedIndex]?.ToString() : null;
        public Action<int>? OnSelectionChanged { get; set; }
        public ColorScheme NormalColors { get; set; } = ColorScheme.Default;
        public ColorScheme FocusColors { get; set; } = ColorScheme.Focus;
        public ColorScheme SelectedColors { get; set; } = ColorScheme.Primary;
        public ColorScheme SelectedFocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.Cyan);

        public ListBox(string id) : base(id)
        {
            Width = 30;
            Height = 10;
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            var colors = HasFocus ? FocusColors : NormalColors;
            
            // Draw border
            renderEngine.DrawBox(X, Y, Width, Height, GetBorderStyle(), colors);

            // Draw items
            int visibleItems = Height - 2;
            for (int i = 0; i < visibleItems; i++)
            {
                int itemIndex = _scrollOffset + i;
                if (itemIndex < _items.Count)
                {
                    int itemY = Y + 1 + i;
                    bool isSelected = itemIndex == _selectedIndex;
                    var itemColors = isSelected 
                        ? (HasFocus ? SelectedFocusColors : SelectedColors)
                        : colors;
                    
                    string displayText = _items[itemIndex]?.ToString() ?? string.Empty;
                    if (displayText.Length > Width - 2)
                    {
                        displayText = displayText.Substring(0, Width - 2);
                    }

                    renderEngine.DrawString(X + 1, itemY, displayText, itemColors);
                    
                    // Draw selection indicator
                    if (isSelected && HasFocus)
                    {
                        renderEngine.DrawChar(X, itemY, '>', itemColors);
                    }
                }
            }

            // Draw scrollbar if needed
            if (_items.Count > visibleItems)
            {
                DrawScrollbar(renderEngine, colors);
            }

            base.Render(renderEngine);
        }

        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) return false;

            if (HandleCommonNavigation(inputEvent))
                return true;

            switch (inputEvent.Key)
            {
                case ConsoleKey.UpArrow:
                    if (_selectedIndex > 0)
                    {
                        SelectedIndex--;
                    }
                    else if (_selectedIndex == -1 && _items.Count > 0)
                    {
                        SelectedIndex = _items.Count - 1;
                    }
                    return true;

                case ConsoleKey.DownArrow:
                    if (_selectedIndex < _items.Count - 1)
                    {
                        SelectedIndex++;
                    }
                    else if (_selectedIndex == -1 && _items.Count > 0)
                    {
                        SelectedIndex = 0;
                    }
                    return true;

                case ConsoleKey.PageUp:
                    if (_items.Count > 0)
                    {
                        SelectedIndex = Math.Max(0, _selectedIndex - (Height - 2));
                    }
                    return true;

                case ConsoleKey.PageDown:
                    if (_items.Count > 0)
                    {
                        SelectedIndex = Math.Min(_items.Count - 1, _selectedIndex + (Height - 2));
                    }
                    return true;

                case ConsoleKey.Home:
                    if (_items.Count > 0)
                    {
                        SelectedIndex = 0;
                    }
                    return true;

                case ConsoleKey.End:
                    if (_items.Count > 0)
                    {
                        SelectedIndex = _items.Count - 1;
                    }
                    return true;
            }

            return false;
        }
        
        protected override bool HandleConfirm()
        {
            if (_selectedIndex >= 0)
            {
                OnSelectionChanged?.Invoke(_selectedIndex);
                return true;
            }
            return false;
        }

        private void EnsureVisible(int index)
        {
            if (index < 0) return;
            
            int visibleItems = Height - 2;
            
            if (index < _scrollOffset)
            {
                _scrollOffset = index;
            }
            else if (index >= _scrollOffset + visibleItems)
            {
                _scrollOffset = index - visibleItems + 1;
            }
        }

        private void DrawScrollbar(IRenderEngine renderEngine, ColorScheme colors)
        {
            int scrollbarHeight = Height - 2;
            int scrollbarX = X + Width - 1;
            
            double visibleRatio = (double)scrollbarHeight / _items.Count;
            int thumbHeight = Math.Max(1, (int)(scrollbarHeight * visibleRatio));
            int thumbPosition = (int)(_scrollOffset * visibleRatio);

            for (int i = 0; i < scrollbarHeight; i++)
            {
                char scrollChar = (i >= thumbPosition && i < thumbPosition + thumbHeight) ? '█' : '│';
                renderEngine.DrawChar(scrollbarX, Y + 1 + i, scrollChar, colors);
            }
        }
        
        private BorderStyle GetBorderStyle()
        {
            return HasFocus ? BorderStyle.Double : BorderStyle.Single;
        }
    }
}