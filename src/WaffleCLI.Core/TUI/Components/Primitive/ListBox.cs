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
                    Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Selection changed to index {_selectedIndex}");
                    OnSelectionChanged?.Invoke(_selectedIndex);
                }
            }
        }

        public string? SelectedItem => _selectedIndex >= 0 ? _items[_selectedIndex]?.ToString() : null;
        public Action<int>? OnSelectionChanged { get; set; }
        public ColorScheme NormalColors { get; set; } = ColorScheme.Default;
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);
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

            int absoluteX = AbsoluteX;
            int absoluteY = AbsoluteY;
            
            var colors = HasFocus ? FocusColors : NormalColors;
            var borderStyle = HasFocus ? BorderStyle.Double : BorderStyle.Single;
            
            renderEngine.DrawBox(absoluteX, absoluteY, Width, Height, borderStyle, colors);

            int visibleItems = Math.Max(0, Height - 2);
            for (int i = 0; i < visibleItems; i++)
            {
                int itemIndex = _scrollOffset + i;
                if (itemIndex >= 0 && itemIndex < _items.Count)
                {
                    int itemY = absoluteY + 1 + i;
                    bool isSelected = itemIndex == _selectedIndex;
                    var itemColors = isSelected 
                        ? (HasFocus ? SelectedFocusColors : SelectedColors)
                        : colors;
                    
                    string displayText = _items[itemIndex]?.ToString() ?? string.Empty;
                    
                    if (displayText.Length > Width - 3)
                    {
                        displayText = displayText.Substring(0, Width - 3) + "...";
                    }

                    if (isSelected && HasFocus)
                    {
                        renderEngine.DrawChar(absoluteX + 1, itemY, '►', itemColors);
                        renderEngine.DrawString(absoluteX + 2, itemY, displayText, itemColors);
                    }
                    else
                    {
                        renderEngine.DrawString(absoluteX + 1, itemY, displayText, itemColors);
                    }
                }
            }

            if (_items.Count > visibleItems)
            {
                DrawScrollbar(renderEngine, colors, absoluteX, absoluteY);
            }

            base.Render(renderEngine);
        }

        private void DrawScrollbar(IRenderEngine renderEngine, ColorScheme colors, int baseX, int baseY)
        {
            int scrollbarHeight = Math.Max(0, Height - 2);
            int scrollbarX = baseX + Width - 1;
            
            if (_items.Count == 0) return;

            double visibleRatio = (double)scrollbarHeight / _items.Count;
            int thumbHeight = Math.Max(1, (int)(scrollbarHeight * visibleRatio));
            int thumbPosition = (int)(_scrollOffset * visibleRatio);

            for (int i = 0; i < scrollbarHeight; i++)
            {
                char scrollChar = (i >= thumbPosition && i < thumbPosition + thumbHeight) ? '█' : '│';
                renderEngine.DrawChar(scrollbarX, baseY + 1 + i, scrollChar, colors);
            }
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
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Move up, selected index: {_selectedIndex}");
                    }
                    else if (_selectedIndex == -1 && _items.Count > 0)
                    {
                        SelectedIndex = _items.Count - 1;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Move to last, selected index: {_selectedIndex}");
                    }
                    return true;

                case ConsoleKey.DownArrow:
                    if (_selectedIndex < _items.Count - 1)
                    {
                        SelectedIndex++;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Move down, selected index: {_selectedIndex}");
                    }
                    else if (_selectedIndex == -1 && _items.Count > 0)
                    {
                        SelectedIndex = 0;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Move to first, selected index: {_selectedIndex}");
                    }
                    return true;

                case ConsoleKey.PageUp:
                    if (_items.Count > 0)
                    {
                        int newIndex = Math.Max(0, _selectedIndex - (Height - 2));
                        SelectedIndex = newIndex;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Page up, selected index: {_selectedIndex}");
                    }
                    return true;

                case ConsoleKey.PageDown:
                    if (_items.Count > 0)
                    {
                        int newIndex = Math.Min(_items.Count - 1, _selectedIndex + (Height - 2));
                        SelectedIndex = newIndex;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Page down, selected index: {_selectedIndex}");
                    }
                    return true;

                case ConsoleKey.Home:
                    if (_items.Count > 0)
                    {
                        SelectedIndex = 0;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Home, selected index: {_selectedIndex}");
                    }
                    return true;

                case ConsoleKey.End:
                    if (_items.Count > 0)
                    {
                        SelectedIndex = _items.Count - 1;
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: End, selected index: {_selectedIndex}");
                    }
                    return true;

                case ConsoleKey.Enter:
                    if (_selectedIndex >= 0)
                    {
                        Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Enter pressed on index {_selectedIndex}");
                        OnSelectionChanged?.Invoke(_selectedIndex);
                        return true;
                    }
                    break;
            }

            return false;
        }
        
        protected override bool HandleConfirm()
        {
            if (_selectedIndex >= 0)
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id}: Confirm selection at index {_selectedIndex}");
                OnSelectionChanged?.Invoke(_selectedIndex);
                return true;
            }
            return false;
        }

        public override void OnFocus()
        {
            base.OnFocus();
            Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id} received focus");
        }

        public override void OnBlur()
        {
            base.OnBlur();
            Infrastructure.Logging.TuiLogger.LogDebug($"ListBox {Id} lost focus");
        }

        private void EnsureVisible(int index)
        {
            if (index < 0) return;
            
            int visibleItems = Math.Max(0, Height - 2);
            
            if (index < _scrollOffset)
            {
                _scrollOffset = index;
            }
            else if (index >= _scrollOffset + visibleItems)
            {
                _scrollOffset = index - visibleItems + 1;
            }
        }

        // private void DrawScrollbar(IRenderEngine renderEngine, ColorScheme colors)
        // {
        //     int scrollbarHeight = Math.Max(0, Height - 2);
        //     int scrollbarX = X + Width - 1;
        //     
        //     if (_items.Count == 0) return;
        //
        //     double visibleRatio = (double)scrollbarHeight / _items.Count;
        //     int thumbHeight = Math.Max(1, (int)(scrollbarHeight * visibleRatio));
        //     int thumbPosition = (int)(_scrollOffset * visibleRatio);
        //
        //     for (int i = 0; i < scrollbarHeight; i++)
        //     {
        //         char scrollChar = (i >= thumbPosition && i < thumbPosition + thumbHeight) ? '█' : '│';
        //         renderEngine.DrawChar(scrollbarX, Y + 1 + i, scrollChar, colors);
        //     }
        // }
    }
}