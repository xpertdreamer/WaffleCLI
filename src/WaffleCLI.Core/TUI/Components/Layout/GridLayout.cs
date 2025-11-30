using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Layout
{
    /// <summary>
    /// Grid layout container
    /// </summary>
    public class GridLayout : ContainerBase
    {
        private readonly List<GridDefinition> _columnDefinitions = new();
        private readonly List<GridDefinition> _rowDefinitions = new();
        private readonly Dictionary<IComponent, GridPosition> _childPositions = new();

        public ColorScheme BackgroundColors { get; set; } = ColorScheme.Default;

        public GridLayout(string id) : base(id)
        {
        }

        public void AddColumn(GridDefinition definition)
        {
            _columnDefinitions.Add(definition);
        }

        public void AddRow(GridDefinition definition)
        {
            _rowDefinitions.Add(definition);
        }

        public void SetChildPosition(IComponent child, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            _childPositions[child] = new GridPosition(column, row, columnSpan, rowSpan);
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            // Draw background
            if (!BackgroundColors.Equals(ColorScheme.Default))
            {
                renderEngine.FillRectangle(X, Y, Width, Height, ' ', BackgroundColors);
            }

            base.Render(renderEngine);
        }

        public override void DoLayout()
        {
            CalculateDefinitions();
            PositionChildren();
        }

        private void CalculateDefinitions()
        {
            // Calculate column widths
            if (_columnDefinitions.Count == 0)
            {
                // Default: equal columns
                int colWidth = Width / Math.Max(1, Children.Count);
                for (int i = 0; i < Children.Count; i++)
                {
                    _columnDefinitions.Add(new GridDefinition { Type = GridUnitType.Star, Value = 1 });
                }
            }

            // Calculate row heights
            if (_rowDefinitions.Count == 0)
            {
                // Default: single row
                _rowDefinitions.Add(new GridDefinition { Type = GridUnitType.Star, Value = 1 });
            }
        }

        private void PositionChildren()
        {
            // This is a simplified implementation
            // In a full implementation, you'd calculate actual pixel sizes based on definitions
            
            int cols = Math.Max(1, _columnDefinitions.Count);
            int rows = Math.Max(1, _rowDefinitions.Count);
            
            int colWidth = Width / cols;
            int rowHeight = Height / rows;

            foreach (var child in Children)
            {
                if (_childPositions.TryGetValue(child, out var position))
                {
                    child.X = X + position.Column * colWidth;
                    child.Y = Y + position.Row * rowHeight;
                    child.Width = colWidth * position.ColumnSpan;
                    child.Height = rowHeight * position.RowSpan;
                }
                else
                {
                    // Default positioning
                    child.X = X;
                    child.Y = Y;
                    child.Width = colWidth;
                    child.Height = rowHeight;
                }
            }
        }
    }

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