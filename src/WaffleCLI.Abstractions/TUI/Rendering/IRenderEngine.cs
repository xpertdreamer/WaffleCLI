using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Abstractions.TUI.Rendering
{
    /// <summary>
    /// Render engine with double buffering support
    /// </summary>
    public interface IRenderEngine : IDisposable
    {
        void Initialize(int width, int height);
        void BeginFrame();
        void EndFrame();
        
        // Drawing primitives
        void DrawString(int x, int y, string text, ColorScheme colors);
        void DrawChar(int x, int y, char character, ColorScheme colors);
        void DrawBox(int x, int y, int width, int height, BorderStyle border, ColorScheme colors);
        void DrawLine(int x1, int y1, int x2, int y2, char lineChar, ColorScheme colors);
        void FillRectangle(int x, int y, int width, int height, char fillChar, ColorScheme colors);
        void Clear(ColorScheme colors);
        
        // Buffer management
        void SetViewport(int x, int y, int width, int height);
        void ResetViewport();
        // void RequestFullRedraw();
    }
}