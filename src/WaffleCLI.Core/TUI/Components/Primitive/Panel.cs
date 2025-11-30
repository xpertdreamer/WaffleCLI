using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Panel container component
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

            // CRITICAL FIX: Render children with proper coordinates
            foreach (var child in Children)
            {
                if (child.IsVisible)
                {
                    child.Render(renderEngine);
                }
            }
        }

        public override void DoLayout()
        {
            // Basic layout - children keep their positions
        }
    }
}