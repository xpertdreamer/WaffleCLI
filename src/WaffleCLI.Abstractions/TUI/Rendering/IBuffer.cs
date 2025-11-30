using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Abstractions.TUI.Rendering
{
    /// <summary>
    /// Buffer interface for rendering
    /// </summary>
    public interface IBuffer : IDisposable
    {
        int Width { get; }
        int Height { get; }
        
        void SetPixel(int x, int y, char character, ConsoleColor foreground, ConsoleColor background);
        void Clear(ColorScheme colors);
        void Swap();
        void RenderToConsole();
    }
}