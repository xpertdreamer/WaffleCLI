using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Configuration;

namespace WaffleCLI.Core.TUI.Components
{
    /// <summary>
    /// Factory for creating UI components with enhanced fluent API
    /// </summary>
    public static class ComponentFactory
    {
        /// <summary>
        /// Creates a new button with fluent configuration
        /// </summary>
        public static ButtonBuilder CreateButton(string id, string text)
        {
            return new ButtonBuilder(id, text);
        }

        /// <summary>
        /// Creates a new label with fluent configuration
        /// </summary>
        public static LabelBuilder CreateLabel(string id, string text)
        {
            return new LabelBuilder(id, text);
        }

        /// <summary>
        /// Creates a new text box with fluent configuration
        /// </summary>
        public static TextBoxBuilder CreateTextBox(string id)
        {
            return new TextBoxBuilder(id);
        }

        /// <summary>
        /// Creates a new list box with fluent configuration
        /// </summary>
        public static ListBoxBuilder CreateListBox(string id)
        {
            return new ListBoxBuilder(id);
        }

        /// <summary>
        /// Creates a new panel with fluent configuration
        /// </summary>
        public static PanelBuilder CreatePanel(string id)
        {
            return new PanelBuilder(id);
        }

        /// <summary>
        /// Creates a new grid layout with fluent configuration
        /// </summary>
        public static GridLayoutBuilder CreateGridLayout(string id)
        {
            return new GridLayoutBuilder(id);
        }

        /// <summary>
        /// Creates a new console panel with fluent configuration
        /// </summary>
        public static ConsolePanelBuilder CreateConsolePanel(string id)
        {
            return new ConsolePanelBuilder(id);
        }

        /// <summary>
        /// Creates a new stack layout with fluent configuration
        /// </summary>
        public static StackLayoutBuilder CreateStackLayout(string id)
        {
            return new StackLayoutBuilder(id);
        }

        // Builder classes for fluent API
        public class ButtonBuilder
        {
            private readonly Button _button;
            
            public ButtonBuilder(string id, string text)
            {
                _button = new Button(id) { Text = text };
            }
            
            public ButtonBuilder WithSize(int width, int height)
            {
                _button.Width = width;
                _button.Height = height;
                return this;
            }
            
            public ButtonBuilder WithColors(ColorScheme colors)
            {
                _button.NormalColors = colors;
                return this;
            }
            
            public ButtonBuilder WithClickHandler(Action handler)
            {
                _button.OnClick = handler;
                return this;
            }
            
            public IButton Build() => _button;
        }

        public class LabelBuilder
        {
            private readonly ImprovedLabel _label;
            
            public LabelBuilder(string id, string text)
            {
                _label = new ImprovedLabel(id) { Text = text };
            }
            
            public LabelBuilder WithAlignment(TextAlignment alignment)
            {
                _label.TextAlignment = alignment;
                return this;
            }
            
            public LabelBuilder WithColors(ColorScheme colors)
            {
                _label.Colors = colors;
                return this;
            }
            
            public ILabel Build() => _label;
        }

        public class TextBoxBuilder
        {
            private readonly TextBox _textBox;
            
            public TextBoxBuilder(string id)
            {
                _textBox = new TextBox(id);
            }
            
            public TextBoxBuilder WithPlaceholder(string placeholder)
            {
                _textBox.Placeholder = placeholder;
                return this;
            }
            
            public TextBoxBuilder WithMaxLength(int maxLength)
            {
                _textBox.MaxLength = maxLength;
                return this;
            }
            
            public TextBoxBuilder WithColors(ColorScheme colors)
            {
                _textBox.NormalColors = colors;
                return this;
            }
            
            public ITextBox Build() => _textBox;
        }

        public class ListBoxBuilder
        {
            private readonly ListBox _listBox;
            
            public ListBoxBuilder(string id)
            {
                _listBox = new ListBox(id);
            }
            
            public ListBoxBuilder WithItems(IEnumerable<object> items)
            {
                _listBox.Items.Clear();
                foreach (var item in items)
                {
                    _listBox.Items.Add(item);
                }
                return this;
            }
            
            public ListBoxBuilder WithSelectionHandler(Action<int> handler)
            {
                _listBox.OnSelectionChanged = handler;
                return this;
            }
            
            public ListBoxBuilder WithSize(int width, int height)
            {
                _listBox.Width = width;
                _listBox.Height = height;
                return this;
            }
            
            public IListBox Build() => _listBox;
        }

        public class PanelBuilder
        {
            private readonly Panel _panel;
            
            public PanelBuilder(string id)
            {
                _panel = new Panel(id);
            }
            
            public PanelBuilder WithSize(int width, int height)
            {
                _panel.Width = width;
                _panel.Height = height;
                return this;
            }
            
            public PanelBuilder WithColors(ColorScheme colors)
            {
                _panel.BackgroundColors = colors;
                return this;
            }
            
            public PanelBuilder WithBorder(BorderStyle border)
            {
                _panel.Border = border;
                return this;
            }
            
            public IContainer Build() => _panel;
        }

        public class GridLayoutBuilder
        {
            private readonly GridLayout _grid;
            
            public GridLayoutBuilder(string id)
            {
                _grid = new GridLayout(id);
            }
            
            public GridLayoutBuilder WithSize(int width, int height)
            {
                _grid.Width = width;
                _grid.Height = height;
                return this;
            }
            
            public GridLayoutBuilder WithColumns(params GridDefinition[] columns)
            {
                foreach (var column in columns)
                {
                    _grid.AddColumn(column);
                }
                return this;
            }
            
            public GridLayoutBuilder WithRows(params GridDefinition[] rows)
            {
                foreach (var row in rows)
                {
                    _grid.AddRow(row);
                }
                return this;
            }
            
            public GridLayoutBuilder WithPadding(int padding)
            {
                _grid.Padding = padding;
                return this;
            }
            
            public GridLayoutBuilder WithSpacing(int horizontal, int vertical)
            {
                _grid.HorizontalSpacing = horizontal;
                _grid.VerticalSpacing = vertical;
                return this;
            }
            
            public GridLayout Build() => _grid;
        }

        public class ConsolePanelBuilder
        {
            private readonly ConsolePanel _console;
            
            public ConsolePanelBuilder(string id)
            {
                _console = new ConsolePanel(id);
            }
            
            public ConsolePanelBuilder WithSize(int width, int height)
            {
                _console.Width = width;
                _console.Height = height;
                return this;
            }
            
            public ConsolePanelBuilder WithPrompt(string prompt)
            {
                _console.Prompt = prompt;
                return this;
            }
            
            public ConsolePanelBuilder WithColors(ColorScheme colors)
            {
                _console.NormalColors = colors;
                return this;
            }
            
            public ConsolePanel Build() => _console;
        }

        public class StackLayoutBuilder
        {
            private readonly StackLayout _stack;
            
            public StackLayoutBuilder(string id)
            {
                _stack = new StackLayout(id);
            }
            
            public StackLayoutBuilder WithOrientation(Orientation orientation)
            {
                _stack.Orientation = orientation;
                return this;
            }
            
            public StackLayoutBuilder WithSpacing(int spacing)
            {
                _stack.Spacing = spacing;
                return this;
            }
            
            public StackLayoutBuilder WithSize(int width, int height)
            {
                _stack.Width = width;
                _stack.Height = height;
                return this;
            }
            
            public StackLayout Build() => _stack;
        }
        
    }
}