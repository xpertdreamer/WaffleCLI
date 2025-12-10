using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Layout
{
    /// <summary>
    /// Simplified grid layout with easy-to-use API
    /// </summary>
    public class SimpleGridLayout : ContainerBase
    {
        private int _columns = 1;
        private int _rows = 1;
        private int _horizontalSpacing = 1;
        private int _verticalSpacing = 0;
        private int _padding = 0;
        private readonly Dictionary<IComponent, GridPos> _childPositions = new();
        private ColorScheme _backgroundColors = ColorScheme.Default;

        /// <summary>
        /// Gets or sets the number of columns
        /// </summary>
        public int Columns
        {
            get => _columns;
            set
            {
                _columns = Math.Max(1, value);
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Gets or sets the number of rows
        /// </summary>
        public int Rows
        {
            get => _rows;
            set
            {
                _rows = Math.Max(1, value);
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Gets or sets horizontal spacing between cells
        /// </summary>
        public int HorizontalSpacing
        {
            get => _horizontalSpacing;
            set
            {
                _horizontalSpacing = Math.Max(0, value);
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Gets or sets vertical spacing between cells
        /// </summary>
        public int VerticalSpacing
        {
            get => _verticalSpacing;
            set
            {
                _verticalSpacing = Math.Max(0, value);
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Gets or sets padding inside cells
        /// </summary>
        public int Padding
        {
            get => _padding;
            set
            {
                _padding = Math.Max(0, value);
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Gets or sets background color scheme
        /// </summary>
        public ColorScheme BackgroundColors
        {
            get => _backgroundColors;
            set
            {
                _backgroundColors = value;
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Initializes a new SimpleGridLayout
        /// </summary>
        public SimpleGridLayout(string id) : base(id)
        {
        }

        /// <summary>
        /// Adds a child with automatic positioning (fills next available cell)
        /// </summary>
        public override void AddChild(IComponent child)
        {
            base.AddChild(child);
            
            // Auto-position child in next available cell
            AutoPositionChild(child);
            InvalidateLayout();
        }

        /// <summary>
        /// Adds a child at specific grid position
        /// </summary>
        public void AddChild(IComponent child, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            base.AddChild(child);
            SetChildPosition(child, column, row, columnSpan, rowSpan);
        }

        /// <summary>
        /// Sets child position in the grid
        /// </summary>
        public void SetChildPosition(IComponent child, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            
            // Clamp values to valid ranges
            column = Math.Clamp(column, 0, _columns - 1);
            row = Math.Clamp(row, 0, _rows - 1);
            columnSpan = Math.Clamp(columnSpan, 1, _columns - column);
            rowSpan = Math.Clamp(rowSpan, 1, _rows - row);
            
            _childPositions[child] = new GridPos(column, row, columnSpan, rowSpan);
            InvalidateLayout();
        }

        /// <summary>
        /// Renders the grid layout
        /// </summary>
        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            // Draw background if not default colors
            if (!_backgroundColors.Equals(ColorScheme.Default))
            {
                renderEngine.FillRectangle(absX, absY, Width, Height, ' ', _backgroundColors);
            }

            // Render children
            base.Render(renderEngine);
        }

        /// <summary>
        /// Performs layout calculations
        /// </summary>
        public override void DoLayout()
        {
            // Safety check: if grid has invalid dimensions, skip layout
            if (Width <= 2 || Height <= 2 || _columns <= 0 || _rows <= 0)
            {
                Infrastructure.Logging.TuiLogger.LogWarning(
                    $"SimpleGridLayout {Id}: Invalid dimensions for layout - W:{Width}, H:{Height}, C:{_columns}, R:{_rows}");
                return;
            }

            // Calculate available space after padding
            int availableWidth = Math.Max(0, Width - (_padding * 2));
            int availableHeight = Math.Max(0, Height - (_padding * 2));

            if (availableWidth <= 0 || availableHeight <= 0)
            {
                Infrastructure.Logging.TuiLogger.LogWarning(
                    $"SimpleGridLayout {Id}: No available space after padding");
                return;
            }

            // Calculate total spacing
            int totalHorizontalSpacing = Math.Max(0, (_columns - 1) * _horizontalSpacing);
            int totalVerticalSpacing = Math.Max(0, (_rows - 1) * _verticalSpacing);

            // Calculate cell dimensions
            int cellWidth = (availableWidth - totalHorizontalSpacing) / _columns;
            int cellHeight = (availableHeight - totalVerticalSpacing) / _rows;

            // Adjust for integer division remainder
            int widthRemainder = availableWidth - (cellWidth * _columns + totalHorizontalSpacing);
            int heightRemainder = availableHeight - (cellHeight * _rows + totalVerticalSpacing);

            // Position each child
            foreach (var child in Children)
            {
                if (!_childPositions.TryGetValue(child, out var position))
                {
                    // Default: child fills entire available space
                    child.X = _padding;
                    child.Y = _padding;
                    child.Width = Math.Max(1, availableWidth);
                    child.Height = Math.Max(1, availableHeight);
                    continue;
                }

                // Validate position
                int col = Math.Clamp(position.Column, 0, _columns - 1);
                int row = Math.Clamp(position.Row, 0, _rows - 1);
                int colSpan = Math.Clamp(position.ColumnSpan, 1, _columns - col);
                int rowSpan = Math.Clamp(position.RowSpan, 1, _rows - row);

                // Calculate position
                int cellX = _padding;
                int cellY = _padding;

                // Add previous columns
                for (int c = 0; c < col; c++)
                {
                    cellX += cellWidth + (c < widthRemainder ? 1 : 0) + _horizontalSpacing;
                }

                // Add previous rows
                for (int r = 0; r < row; r++)
                {
                    cellY += cellHeight + (r < heightRemainder ? 1 : 0) + _verticalSpacing;
                }

                // Calculate spanned dimensions
                int spannedWidth = 0;
                for (int c = 0; c < colSpan; c++)
                {
                    int currentCol = col + c;
                    spannedWidth += cellWidth + (currentCol < widthRemainder ? 1 : 0);
                    if (c > 0) spannedWidth += _horizontalSpacing;
                }

                int spannedHeight = 0;
                for (int r = 0; r < rowSpan; r++)
                {
                    int currentRow = row + r;
                    spannedHeight += cellHeight + (currentRow < heightRemainder ? 1 : 0);
                    if (r > 0) spannedHeight += _verticalSpacing;
                }

                // Set child dimensions with bounds checking
                child.X = Math.Max(_padding, Math.Min(cellX, Width - _padding - 1));
                child.Y = Math.Max(_padding, Math.Min(cellY, Height - _padding - 1));
                child.Width = Math.Max(1, Math.Min(spannedWidth, Width - child.X));
                child.Height = Math.Max(1, Math.Min(spannedHeight, Height - child.Y));
            }
        }

        private void AutoPositionChild(IComponent child)
        {
            // Find first available cell
            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    bool cellOccupied = false;
                    
                    foreach (var existing in _childPositions)
                    {
                        var pos = existing.Value;
                        if (col >= pos.Column && col < pos.Column + pos.ColumnSpan &&
                            row >= pos.Row && row < pos.Row + pos.RowSpan)
                        {
                            cellOccupied = true;
                            break;
                        }
                    }
                    
                    if (!cellOccupied)
                    {
                        SetChildPosition(child, col, row);
                        return;
                    }
                }
            }
            
            // If no cells available, add to the end (will be positioned by default logic)
            Infrastructure.Logging.TuiLogger.LogWarning($"Grid {Id} is full, child {child.Id} will fill entire grid");
        }

        /// <summary>
        /// Removes a child from the grid
        /// </summary>
        public override void RemoveChild(IComponent child)
        {
            base.RemoveChild(child);
            _childPositions.Remove(child);
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Grid position for a child component
    /// </summary>
    public struct GridPos
    {
        /// <summary>
        /// Column index (0-based)
        /// </summary>
        public int Column { get; }
        
        /// <summary>
        /// Row index (0-based)
        /// </summary>
        public int Row { get; }
        
        /// <summary>
        /// Number of columns to span
        /// </summary>
        public int ColumnSpan { get; }
        
        /// <summary>
        /// Number of rows to span
        /// </summary>
        public int RowSpan { get; }

        /// <summary>
        /// Initializes a new grid position
        /// </summary>
        public GridPos(int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            Column = column;
            Row = row;
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }
    }
}