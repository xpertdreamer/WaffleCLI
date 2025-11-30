using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Abstractions.TUI.Rendering
{
    /// <summary>
    /// Context for rendering operations
    /// </summary>
    public class RenderContext
    {
        public int ViewportX { get; set; }
        public int ViewportY { get; set; }
        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }
        public bool ViewportActive { get; set; }
        public ColorScheme CurrentColors { get; set; } = ColorScheme.Default;
        
        public bool IsInViewport(int x, int y)
        {
            if (!ViewportActive) return true;
            
            return x >= ViewportX && x < ViewportX + ViewportWidth &&
                   y >= ViewportY && y < ViewportY + ViewportHeight;
        }
    }
}