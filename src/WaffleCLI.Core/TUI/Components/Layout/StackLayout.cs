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
                renderEngine.FillRectangle(X, Y, Width, Height, ' ', BackgroundColors);
            }

            base.Render(renderEngine);
        }

        public override void DoLayout()
        {
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
            int currentY = Y;
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                child.X = X;
                child.Y = currentY;
                child.Width = Width;
                currentY += child.Height + Spacing;
            }
        }

        private void DoHorizontalLayout()
        {
            int currentX = X;
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                child.X = currentX;
                child.Y = Y;
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