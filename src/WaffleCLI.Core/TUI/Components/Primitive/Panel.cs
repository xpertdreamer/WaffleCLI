using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Panel container component with resize support
    /// </summary>
    public class Panel : ContainerBase
    {
        public ColorScheme BackgroundColors { get; set; } = ColorScheme.Default;
        public BorderStyle Border { get; set; } = BorderStyle.None;
        public ColorScheme BorderColors { get; set; } = ColorScheme.Default;

        public Panel(string id) : base(id)
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

            // Draw border
            if (Border != BorderStyle.None)
            {
                renderEngine.DrawBox(X, Y, Width, Height, Border, BorderColors);
            }

            base.Render(renderEngine);
        }

        public override void DoLayout()
        {
            // Basic layout - children keep their positions
            // This can be enhanced with layout managers
            foreach (var child in Children)
            {
                // Ensure children don't exceed panel bounds
                if (child.X + child.Width > Width)
                {
                    child.Width = Math.Max(1, Width - child.X);
                }
                if (child.Y + child.Height > Height)
                {
                    child.Height = Math.Max(1, Height - child.Y);
                }
            }
        }

        public override void Update()
        {
            // Update layout when dimensions change
            DoLayout();
            base.Update();
        }
    }
}