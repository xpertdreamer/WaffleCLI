using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Layout
{
    /// <summary>
    /// Stack layout container
    /// </summary>
    public class StackLayout : ContainerBase
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;
        public int Spacing { get; set; } = 0;
        public ColorScheme BackgroundColors { get; set; } = ColorScheme.Default;

        public StackLayout(string id) : base(id)
        {
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            // Draw background
            if (!BackgroundColors.Equals(ColorScheme.Default))
            {
                renderEngine.FillRectangle(AbsoluteX, AbsoluteY, Width, Height, ' ', BackgroundColors);
            }

            base.Render(renderEngine);
        }

        public override void DoLayout()
        {
            if (Children.Count == 0) return;

            if (Orientation == Orientation.Vertical)
            {
                DoVerticalLayout();
            }
            else
            {
                DoHorizontalLayout();
            }
        }

        private void DoVerticalLayout()
        {
            int currentY = 0;
            int availableHeight = Height;
            int totalSpacing = Math.Max(0, (Children.Count - 1) * Spacing);
            int childHeight = (availableHeight - totalSpacing) / Children.Count;
            
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                child.X = 0;
                child.Y = currentY;
                child.Width = Width;
                child.Height = Math.Max(1, childHeight);
                currentY += child.Height + Spacing;
            }
        }

        private void DoHorizontalLayout()
        {
            int currentX = 0;
            int availableWidth = Width;
            int totalSpacing = Math.Max(0, (Children.Count - 1) * Spacing);
            int childWidth = (availableWidth - totalSpacing) / Children.Count;
            
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                child.X = currentX;
                child.Y = 0;
                child.Width = Math.Max(1, childWidth);
                child.Height = Height;
                currentX += child.Width + Spacing;
            }
        }
    }

    public enum Orientation
    {
        Horizontal,
        Vertical
    }
}