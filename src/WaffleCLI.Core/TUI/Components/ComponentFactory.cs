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
    /// Factory for creating UI components with fluent API
    /// </summary>
    public static class ComponentFactory
    {
        /// <summary>
        /// Creates a new button
        /// </summary>
        public static IButton CreateButton(string id, string text, Action onClick = null)
        {
            var button = new Button(id)
            {
                Text = text,
                OnClick = onClick
            };
            
            return button;
        }

        /// <summary>
        /// Creates a new label
        /// </summary>
        public static ILabel CreateLabel(string id, string text, TextAlignment alignment = TextAlignment.Left)
        {
            var label = new ImprovedLabel(id)
            {
                Text = text,
                TextAlignment = alignment
            };
            
            return label;
        }

        /// <summary>
        /// Creates a new text box
        /// </summary>
        public static ITextBox CreateTextBox(string id, string placeholder = "", int maxLength = 256)
        {
            var textBox = new TextBox(id)
            {
                Placeholder = placeholder,
                MaxLength = maxLength
            };
            
            return textBox;
        }

        /// <summary>
        /// Creates a new list box
        /// </summary>
        public static IListBox CreateListBox(string id, IEnumerable<object> items = null)
        {
            var listBox = new ListBox(id);
            
            if (items != null)
            {
                listBox.Items.Clear();
                foreach (var item in items)
                {
                    listBox.Items.Add(item);
                }
            }
            
            return listBox;
        }

        /// <summary>
        /// Creates a new panel
        /// </summary>
        public static IContainer CreatePanel(string id, ColorScheme? backgroundColors = null, BorderStyle border = BorderStyle.None)
        {
            var panel = new Panel(id);
            
            if (backgroundColors.HasValue)
            {
                panel.BackgroundColors = backgroundColors.Value;
            }
            
            panel.Border = border;
            
            return panel;
        }

        /// <summary>
        /// Creates a new simple grid layout
        /// </summary>
        public static SimpleGridLayout CreateGrid(string id, int columns = 1, int rows = 1)
        {
            var grid = new SimpleGridLayout(id)
            {
                Columns = columns,
                Rows = rows
            };
            
            return grid;
        }

        /// <summary>
        /// Creates a new stack layout
        /// </summary>
        public static StackLayout CreateStack(string id, Orientation orientation = Orientation.Vertical, int spacing = 0)
        {
            var stack = new StackLayout(id)
            {
                Orientation = orientation,
                Spacing = spacing
            };
            
            return stack;
        }

        /// <summary>
        /// Creates a new console panel
        /// </summary>
        public static ConsolePanel CreateConsolePanel(string id, string prompt = "> ", bool showPrompt = true)
        {
            var console = new ConsolePanel(id)
            {
                Prompt = prompt,
                ShowPrompt = showPrompt
            };
            
            return console;
        }

        /// <summary>
        /// Configures component properties using fluent API
        /// </summary>
        public static T WithPosition<T>(this T component, int x, int y) where T : IComponent
        {
            component.X = x;
            component.Y = y;
            return component;
        }

        /// <summary>
        /// Configures component size using fluent API
        /// </summary>
        public static T WithSize<T>(this T component, int width, int height) where T : IComponent
        {
            component.Width = width;
            component.Height = height;
            return component;
        }

        /// <summary>
        /// Configures component colors using fluent API
        /// </summary>
        public static T WithColors<T>(this T component, ColorScheme colors) where T : IComponent
        {
            if (component is Button button)
            {
                button.NormalColors = colors;
            }
            else if (component is TextBox textBox)
            {
                textBox.NormalColors = colors;
            }
            else if (component is ListBox listBox)
            {
                listBox.NormalColors = colors;
            }
            else if (component is Panel panel)
            {
                panel.BackgroundColors = colors;
            }
            else if (component is ImprovedLabel label)
            {
                label.Colors = colors;
            }
            else if (component is ContainerBase container && container is SimpleGridLayout grid)
            {
                grid.BackgroundColors = colors;
            }
            else if (component is ContainerBase container2 && container2 is StackLayout stack)
            {
                stack.BackgroundColors = colors;
            }
            
            return component;
        }
        
        /// <summary>
        /// Creates a test grid for debugging layout issues
        /// </summary>
        public static SimpleGridLayout CreateTestGrid(string id, int childrenCount = 4)
        {
            var grid = CreateGrid(id, 2, 2)
                .WithSize(80, 24)
                .WithColors(new ColorScheme(ConsoleColor.White, ConsoleColor.Black))
                .WithSpacing(1, 0)
                .WithPadding(1);
        
            // Add test children with different colors
            var colors = new[]
            {
                new ColorScheme(ConsoleColor.Black, ConsoleColor.Red),
                new ColorScheme(ConsoleColor.Black, ConsoleColor.Green),
                new ColorScheme(ConsoleColor.Black, ConsoleColor.Blue),
                new ColorScheme(ConsoleColor.Black, ConsoleColor.Yellow)
            };
        
            for (int i = 0; i < Math.Min(childrenCount, 4); i++)
            {
                int row = i / 2;
                int col = i % 2;
            
                var label = CreateLabel($"test{i}", $"Cell {i+1}\n({col},{row})")
                    .WithColors(colors[i % colors.Length]);
            
                grid.AddToGrid(label, col, row);
            }
        
            return grid;
        }
    
        /// <summary>
        /// Configures grid spacing using fluent API
        /// </summary>
        public static SimpleGridLayout WithSpacing(this SimpleGridLayout grid, 
            int horizontal, int vertical)
        {
            grid.HorizontalSpacing = horizontal;
            grid.VerticalSpacing = vertical;
            return grid;
        }
    
        /// <summary>
        /// Configures grid padding using fluent API
        /// </summary>
        public static SimpleGridLayout WithPadding(this SimpleGridLayout grid, int padding)
        {
            grid.Padding = padding;
            return grid;
        }

        /// <summary>
        /// Adds a child to a container using fluent API
        /// </summary>
        public static TContainer AddChild<TContainer>(this TContainer container, IComponent child) 
            where TContainer : IContainer
        {
            container.AddChild(child);
            return container;
        }

        /// <summary>
        /// Adds a child to a simple grid at specific position using fluent API
        /// </summary>
        public static SimpleGridLayout AddToGrid(this SimpleGridLayout grid, IComponent child, 
            int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            grid.AddChild(child, column, row, columnSpan, rowSpan);
            return grid;
        }
        
        /// <summary>
        /// Creates a selection handler for list box
        /// </summary>
        public static IListBox WithSelectionHandler(this IListBox listBox, Action<int> handler)
        {
            if (listBox is ListBox concreteListBox)
            {
                concreteListBox.OnSelectionChanged = handler;
            }
            return listBox;
        }
    
        /// <summary>
        /// Creates a modern binary launcher
        /// </summary>
        public static BinaryLauncherNew CreateBinaryLauncher(string id, 
            BinariesManager binariesManager, ConsolePanel consolePanel)
        {
            return new BinaryLauncherNew(id, binariesManager, consolePanel);
        }
    
        /// <summary>
        /// Creates a modern demo application
        /// </summary>
        public static BinaryDemoNewApp CreateBinaryDemo(BinariesManager binariesManager, 
            SettingsManager settingsManager)
        {
            return new BinaryDemoNewApp(binariesManager, settingsManager);
        }
    
        /// <summary>
        /// Configures grid rows and columns
        /// </summary>
        public static SimpleGridLayout WithGrid(this SimpleGridLayout grid, int columns, int rows)
        {
            grid.Columns = columns;
            grid.Rows = rows;
            return grid;
        }
    }
}