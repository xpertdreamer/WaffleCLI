// File: WaffleCLI.Core.TUI/Components/Layout/GridLayout.cs (улучшенная версия)
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Layout
{
    /// <summary>
    /// Improved GridLayout with boundary checking and proper proportional calculations
    /// </summary>
    public class GridLayout : ContainerBase
    {
        private readonly List<GridDefinition> _columnDefinitions = new();
        private readonly List<GridDefinition> _rowDefinitions = new();
        private readonly Dictionary<IComponent, GridPosition> _childPositions = new();
        private readonly List<IComponent> _visibleChildren = new();

        public ColorScheme BackgroundColors { get; set; } = ColorScheme.Default;
        public int Padding { get; set; } = 0;
        public int HorizontalSpacing { get; set; } = 0;
        public int VerticalSpacing { get; set; } = 0;

        public GridLayout(string id) : base(id)
        {
        }

        /// <summary>
        /// Adds a column definition with proper validation
        /// </summary>
        public void AddColumn(GridDefinition definition)
        {
            if (definition.Type == GridUnitType.Star && definition.Value <= 0)
                definition.Value = 1;
            
            _columnDefinitions.Add(definition);
            InvalidateLayout();
        }

        /// <summary>
        /// Adds a row definition with proper validation
        /// </summary>
        public void AddRow(GridDefinition definition)
        {
            if (definition.Type == GridUnitType.Star && definition.Value <= 0)
                definition.Value = 1;
            
            _rowDefinitions.Add(definition);
            InvalidateLayout();
        }

        /// <summary>
        /// Sets child position with boundary validation
        /// </summary>
        public void SetChildPosition(IComponent child, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            
            // Validate boundaries
            int maxColumn = Math.Max(1, _columnDefinitions.Count) - 1;
            int maxRow = Math.Max(1, _rowDefinitions.Count) - 1;
            
            column = Math.Clamp(column, 0, maxColumn);
            row = Math.Clamp(row, 0, maxRow);
            columnSpan = Math.Clamp(columnSpan, 1, maxColumn - column + 1);
            rowSpan = Math.Clamp(rowSpan, 1, maxRow - row + 1);
            
            _childPositions[child] = new GridPosition(column, row, columnSpan, rowSpan);
            InvalidateLayout();
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            // Use absolute coordinates for rendering
            int absX = AbsoluteX;
            int absY = AbsoluteY;
    
            // Draw background if colors are not default
            if (!BackgroundColors.Equals(ColorScheme.Default))
            {
                renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);
            }
    
            // Draw border around GridLayout
            renderEngine.DrawBox(absX, absY, Width, Height, BorderStyle.Single, 
                new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue));

            // Render only visible children
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                child.Render(renderEngine);
            }
        }

        public override void DoLayout()
        {
            CalculateDefinitions();
            PositionChildren();
            UpdateVisibleChildren();
        }

        private void CalculateDefinitions()
        {
            // Ensure we have at least one column and row
            if (_columnDefinitions.Count == 0)
            {
                _columnDefinitions.Add(new GridDefinition { Type = GridUnitType.Star, Value = 1 });
            }

            if (_rowDefinitions.Count == 0)
            {
                _rowDefinitions.Add(new GridDefinition { Type = GridUnitType.Star, Value = 1 });
            }
        }

        private void PositionChildren()
        {
            int cols = _columnDefinitions.Count;
            int rows = _rowDefinitions.Count;
            
            // Calculate available space after padding
            int availableWidth = Math.Max(0, Width - (Padding * 2));
            int availableHeight = Math.Max(0, Height - (Padding * 2));
            
            // Calculate column widths
            int[] columnWidths = CalculateColumnWidths(availableWidth);
            int[] rowHeights = CalculateRowHeights(availableHeight);
            
            // Calculate starting positions
            int startX = Padding;
            int startY = Padding;
            
            // Position each child
            foreach (var child in Children)
            {
                if (_childPositions.TryGetValue(child, out var position))
                {
                    // Calculate child position
                    int childX = startX;
                    int childY = startY;
                    int childWidth = 0;
                    int childHeight = 0;
                    
                    // Calculate X position and width
                    for (int col = 0; col < position.Column; col++)
                    {
                        if (col < columnWidths.Length)
                        {
                            childX += columnWidths[col];
                            if (col > 0) childX += HorizontalSpacing;
                        }
                    }
                    
                    for (int col = 0; col < position.ColumnSpan; col++)
                    {
                        int colIndex = position.Column + col;
                        if (colIndex < columnWidths.Length)
                        {
                            childWidth += columnWidths[colIndex];
                            if (col > 0) childWidth += HorizontalSpacing;
                        }
                    }
                    
                    // Calculate Y position and height
                    for (int row = 0; row < position.Row; row++)
                    {
                        if (row < rowHeights.Length)
                        {
                            childY += rowHeights[row];
                            if (row > 0) childY += VerticalSpacing;
                        }
                    }
                    
                    for (int row = 0; row < position.RowSpan; row++)
                    {
                        int rowIndex = position.Row + row;
                        if (rowIndex < rowHeights.Length)
                        {
                            childHeight += rowHeights[rowIndex];
                            if (row > 0) childHeight += VerticalSpacing;
                        }
                    }
                    
                    // Apply calculated dimensions with bounds checking
                    child.X = Math.Max(0, childX);
                    child.Y = Math.Max(0, childY);
                    child.Width = Math.Max(1, Math.Min(childWidth, Width - child.X));
                    child.Height = Math.Max(1, Math.Min(childHeight, Height - child.Y));
                }
                else
                {
                    // Default positioning if no specific position set
                    child.X = Padding;
                    child.Y = Padding;
                    child.Width = Math.Max(1, availableWidth);
                    child.Height = Math.Max(1, availableHeight);
                }
            }
        }

        private int[] CalculateColumnWidths(int availableWidth)
        {
            int cols = _columnDefinitions.Count;
            int[] widths = new int[cols];
            
            // First pass: calculate fixed and auto sizes
            int remainingWidth = availableWidth;
            int starCount = 0;
            double starValueSum = 0;
            
            for (int i = 0; i < cols; i++)
            {
                var def = _columnDefinitions[i];
                
                switch (def.Type)
                {
                    case GridUnitType.Pixel:
                        widths[i] = (int)def.Value;
                        remainingWidth -= widths[i];
                        break;
                    case GridUnitType.Auto:
                        // Auto columns will be calculated later
                        widths[i] = 0;
                        break;
                    case GridUnitType.Star:
                        starCount++;
                        starValueSum += def.Value;
                        break;
                }
                
                // Subtract spacing (except for last column)
                if (i < cols - 1)
                {
                    remainingWidth -= HorizontalSpacing;
                }
            }
            
            // Second pass: calculate star sizes
            if (starCount > 0 && remainingWidth > 0)
            {
                double starUnit = remainingWidth / starValueSum;
                
                for (int i = 0; i < cols; i++)
                {
                    if (_columnDefinitions[i].Type == GridUnitType.Star)
                    {
                        widths[i] = (int)(_columnDefinitions[i].Value * starUnit);
                        remainingWidth -= widths[i];
                    }
                }
            }
            
            // Distribute any remaining space
            if (remainingWidth > 0 && cols > 0)
            {
                widths[cols - 1] += remainingWidth;
            }
            
            return widths;
        }

        private int[] CalculateRowHeights(int availableHeight)
        {
            int rows = _rowDefinitions.Count;
            int[] heights = new int[rows];
            
            // Similar logic to column widths
            int remainingHeight = availableHeight;
            int starCount = 0;
            double starValueSum = 0;
            
            for (int i = 0; i < rows; i++)
            {
                var def = _rowDefinitions[i];
                
                switch (def.Type)
                {
                    case GridUnitType.Pixel:
                        heights[i] = (int)def.Value;
                        remainingHeight -= heights[i];
                        break;
                    case GridUnitType.Auto:
                        heights[i] = 0;
                        break;
                    case GridUnitType.Star:
                        starCount++;
                        starValueSum += def.Value;
                        break;
                }
                
                if (i < rows - 1)
                {
                    remainingHeight -= VerticalSpacing;
                }
            }
            
            if (starCount > 0 && remainingHeight > 0)
            {
                double starUnit = remainingHeight / starValueSum;
                
                for (int i = 0; i < rows; i++)
                {
                    if (_rowDefinitions[i].Type == GridUnitType.Star)
                    {
                        heights[i] = (int)(_rowDefinitions[i].Value * starUnit);
                        remainingHeight -= heights[i];
                    }
                }
            }
            
            if (remainingHeight > 0 && rows > 0)
            {
                heights[rows - 1] += remainingHeight;
            }
            
            return heights;
        }

        private void UpdateVisibleChildren()
        {
            _visibleChildren.Clear();
            
            foreach (var child in Children)
            {
                if (child.IsVisible && IsChildInBounds(child))
                {
                    _visibleChildren.Add(child);
                }
            }
        }

        private bool IsChildInBounds(IComponent child)
        {
            // Check if child is within this GridLayout's bounds
            return child.AbsoluteX >= AbsoluteX &&
                   child.AbsoluteY >= AbsoluteY &&
                   child.AbsoluteX + child.Width <= AbsoluteX + Width &&
                   child.AbsoluteY + child.Height <= AbsoluteY + Height;
        }

        private void InvalidateLayout()
        {
            // Mark layout as dirty
            if (IsVisible)
            {
                DoLayout();
            }
        }

        public override void AddChild(IComponent child)
        {
            base.AddChild(child);
            InvalidateLayout();
        }

        public override void RemoveChild(IComponent child)
        {
            base.RemoveChild(child);
            _childPositions.Remove(child);
            _visibleChildren.Remove(child);
            InvalidateLayout();
        }
    }

    // GridDefinition and GridPosition structs remain the same
    public struct GridDefinition
    {
        public GridUnitType Type { get; set; }
        public double Value { get; set; }
    }

    public enum GridUnitType
    {
        Auto,
        Pixel,
        Star
    }

    public struct GridPosition
    {
        public int Column { get; }
        public int Row { get; }
        public int ColumnSpan { get; }
        public int RowSpan { get; }

        public GridPosition(int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            Column = column;
            Row = row;
            ColumnSpan = columnSpan;
            RowSpan = rowSpan;
        }
    }
}