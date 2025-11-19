namespace WaffleCLI.Core.TUI;

/// <summary>
/// Provides a base class for all Text User Interface elements with common rendering and input handling capabilities.
/// </summary>
/// <remarks>
/// This abstract class defines the fundamental properties and methods for TUI elements,
/// including positioning, colors, rendering, and keyboard input handling. It also provides
/// utility methods for common drawing operations like boxes and borders.
/// </remarks>
public abstract class TuiElement
{
    /// <summary>
    /// Gets or sets the horizontal position of the element relative to the console window.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the vertical position of the element relative to the console window.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the element in characters.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the element in characters.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the background color of the element.
    /// </summary>
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    /// <summary>
    /// Gets or sets the foreground (text) color of the element.
    /// </summary>
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// Renders the element to the console.
    /// </summary>
    /// <remarks>
    /// This abstract method must be implemented by derived classes to define
    /// the specific visual representation of the element.
    /// </remarks>
    public abstract void Render();

    /// <summary>
    /// Handles keyboard input for the element.
    /// </summary>
    /// <param name="keyInfo">The console key information containing the pressed key and modifiers.</param>
    /// <returns>True if the key was handled by this element; otherwise, false.</returns>
    /// <remarks>
    /// This virtual method provides a default implementation that returns false.
    /// Derived classes should override this method to handle specific keyboard input.
    /// </remarks>
    public virtual bool HandleKey(ConsoleKeyInfo keyInfo) => false;

    /// <summary>
    /// Sets the cursor position relative to the element's position.
    /// </summary>
    /// <param name="x">The horizontal offset from the element's X position.</param>
    /// <param name="y">The vertical offset from the element's Y position.</param>
    /// <remarks>
    /// This method performs bounds checking to ensure the cursor position is within the console window.
    /// </remarks>
    protected void SetCursorPosition(int x, int y)
    {
        if (x >= 0 && x < Console.WindowWidth && y >= 0 && y < Console.WindowHeight)
        {
            Console.SetCursorPosition(X + x, Y + y);
        }
    }

    /// <summary>
    /// Draws a box with borders and an optional title at the specified position.
    /// </summary>
    /// <param name="x">The horizontal position relative to the element.</param>
    /// <param name="y">The vertical position relative to the element.</param>
    /// <param name="width">The width of the box in characters.</param>
    /// <param name="height">The height of the box in characters.</param>
    /// <param name="title">The optional title to display in the top border.</param>
    /// <remarks>
    /// This method draws a box using ASCII characters for borders. The title is centered
    /// in the top border if provided. The box includes proper corner characters and borders.
    /// </remarks>
    protected void DrawBox(int x, int y, int width, int height, string title = "")
    {
        SetCursorPosition(x, y);
        Console.Write("+");
        for (int i = 0; i < width - 2; i++) Console.Write("-");
        Console.Write("+");
        
        for (int i = 1; i < height - 1; i++)
        {
            SetCursorPosition(x, y + i);
            Console.Write("|");
            SetCursorPosition(x + width - 1, y + i);
            Console.Write("|");
        }
        
        SetCursorPosition(x, y + height - 1);
        Console.Write("+");
        for (int i = 0; i < width - 2; i++) Console.Write("-");
        Console.Write("+");

        if (string.IsNullOrEmpty(title)) return;
        SetCursorPosition(x + 2, y);
        Console.Write($" {title} ");
    }
}