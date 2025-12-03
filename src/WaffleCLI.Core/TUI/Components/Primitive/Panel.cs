using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Panel container with border and background
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

            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            // Draw background
            if (!BackgroundColors.Equals(ColorScheme.Default))
            {
                renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);
            }

            // Draw border
            if (Border != BorderStyle.None)
            {
                renderEngine.DrawBox(absX, absY, Width, Height, Border, BorderColors);
            }

            base.Render(renderEngine);
        }

        public override void DoLayout()
        {
            // Ensure children fit within panel
            FitChildrenToContainer();
            
            // Validate all children bounds
            foreach (var child in Children.OfType<ComponentBase>())
            {
                if (!child.ValidateBounds())
                {
                    Infrastructure.Logging.TuiLogger.LogWarning(
                        $"Child {child.Id} exceeds panel {Id} bounds");
                }
            }
        }
    }
}