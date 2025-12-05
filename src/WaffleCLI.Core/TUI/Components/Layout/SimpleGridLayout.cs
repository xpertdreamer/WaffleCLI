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

            // Calculate cell dimensions with safety checks
            int totalHorizontalSpacing = Math.Max(0, (_columns - 1) * _horizontalSpacing);
            int totalVerticalSpacing = Math.Max(0, (_rows - 1) * _verticalSpacing);

            int availableWidth = Math.Max(1, Width - (_padding * 2) - totalHorizontalSpacing);
            int availableHeight = Math.Max(1, Height - (_padding * 2) - totalVerticalSpacing);

            if (availableWidth <= 0 || availableHeight <= 0)
            {
                Infrastructure.Logging.TuiLogger.LogWarning(
                    $"SimpleGridLayout {Id}: No available space after padding/spacing");
                return;
            }

            int cellWidth = availableWidth / _columns;
            int cellHeight = availableHeight / _rows;

            // Distribute remaining space
            int remainingWidth = Math.Max(0, availableWidth - (cellWidth * _columns));
            int remainingHeight = Math.Max(0, availableHeight - (cellHeight * _rows));

            // Position each child with bounds checking
            foreach (var child in Children)
            {
                if (!_childPositions.TryGetValue(child, out var position))
                {
                    // Default: child fills entire grid (with padding)
                    child.X = _padding;
                    child.Y = _padding;
                    child.Width = Math.Max(1, availableWidth);
                    child.Height = Math.Max(1, availableHeight);
                    continue;
                }

                // Clamp position to valid ranges
                int col = Math.Clamp(position.Column, 0, _columns - 1);
                int row = Math.Clamp(position.Row, 0, _rows - 1);
                int colSpan = Math.Clamp(position.ColumnSpan, 1, _columns - col);
                int rowSpan = Math.Clamp(position.RowSpan, 1, _rows - row);

                // Calculate starting position
                int cellX = _padding + (col * (cellWidth + _horizontalSpacing)) +
                            Math.Min(col, remainingWidth);
                int cellY = _padding + (row * (cellHeight + _verticalSpacing)) +
                            Math.Min(row, remainingHeight);

                // Calculate dimensions for spanned cells
                int spannedWidth = 0;
                for (int c = 0; c < colSpan; c++)
                {
                    int currentCol = col + c;
                    int currentCellWidth = cellWidth + (currentCol < remainingWidth ? 1 : 0);
                    spannedWidth += currentCellWidth;

                    if (c > 0 && _horizontalSpacing > 0)
                    {
                        spannedWidth += _horizontalSpacing;
                    }
                }

                int spannedHeight = 0;
                for (int r = 0; r < rowSpan; r++)
                {
                    int currentRow = row + r;
                    int currentCellHeight = cellHeight + (currentRow < remainingHeight ? 1 : 0);
                    spannedHeight += currentCellHeight;

                    if (r > 0 && _verticalSpacing > 0)
                    {
                        spannedHeight += _verticalSpacing;
                    }
                }

                // Ensure child stays within grid bounds
                int finalX = Math.Max(_padding, Math.Min(cellX, Width - _padding - 1));
                int finalY = Math.Max(_padding, Math.Min(cellY, Height - _padding - 1));
                int finalWidth = Math.Max(1, Math.Min(spannedWidth, Width - finalX));
                int finalHeight = Math.Max(1, Math.Min(spannedHeight, Height - finalY));

                child.X = finalX;
                child.Y = finalY;
                child.Width = finalWidth;
                child.Height = finalHeight;

                // Debug logging
                Infrastructure.Logging.TuiLogger.LogDebug(
                    $"SimpleGridLayout {Id}: Child {child.Id} at ({finalX},{finalY}) size {finalWidth}x{finalHeight}");
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