namespace WaffleCLI.Abstractions.TUI;

public interface IRenderEngine
{
    void Initialize(int width, int height);
    void BeginFrame();
    void EndFrame();
    void RenderElement(ITuiElement element);
    void RenderText(int x, int y, string text, ConsoleColor fg, ConsoleColor bg);
    void RenderRect(int x, int y, int width, int height, ConsoleColor color, char fillChar = ' ');
    void RenderBorder(int x, int y, int width, int height, BorderStyle borderStyle);
    void Clear();
    void ClearArea(int x, int y, int width, int height);
    void SetCursorPosition(int x, int y);
    void Flush();
    
    int Width { get; }
    int Height { get; }
    bool SupportsPartialRendering { get; }
}

public enum BorderStyle
{
    Single,
    Double,
    Rounded,
    Thick,
    Dashed
}

public record RenderStats (
    int ElementRendered,
    int CharactersDrawn,
    int DirtyRegion,
    double RenderTimeMs,
    double FlushTimeMs
);