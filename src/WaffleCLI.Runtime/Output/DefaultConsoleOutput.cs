using System.Drawing;
using WaffleCLI.Core.Output;

namespace WaffleCLI.Runtime.Output;

/// <summary>
/// Default implementation of <see cref="IConsoleOutput"/> that provides colored console output
/// using standard system console with predefined color schemes for different message types.
/// </summary>
public class DefaultConsoleOutput : IConsoleOutput
{
    /// <summary>
    /// Writes the specified text to the standard output stream without a line terminator.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    public void Write(string text)
    {
        Console.Write(text);
    }

    public void Write(string text, ConsoleColor color)
    {
        var originalColor =  Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Writes the specified text to the standard output stream followed by a line terminator.
    /// </summary>
    /// <param name="text">The text to write to the console. If empty, writes just a line terminator.</param>
    public void WriteLine(string text = "")
    {
        Console.WriteLine(text);
    }

    /// <summary>
    /// Writes the specified text to the standard output stream with the specified foreground color,
    /// followed by a line terminator. Restores the original console color after writing.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    /// <param name="color">The foreground color to use for the text.</param>
    public void WriteLine(string text, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Writes an error message to the standard output stream in red color.
    /// </summary>
    /// <param name="text">The error message to display.</param>
    public void WriteError(string text)
    {
        WriteLine(text, ConsoleColor.Red);
    }

    /// <summary>
    /// Writes a warning message to the standard output stream in yellow color.
    /// </summary>
    /// <param name="text">The warning message to display.</param>
    public void WriteWarning(string text)
    {
        WriteLine(text, ConsoleColor.Yellow);
    }

    /// <summary>
    /// Writes an informational message to the standard output stream in cyan color.
    /// </summary>
    /// <param name="text">The informational message to display.</param>
    public void WriteInfo(string text)
    {
        WriteLine(text, ConsoleColor.Cyan);
    }

    /// <summary>
    /// Writes a success message to the standard output stream in green color.
    /// </summary>
    /// <param name="text">The success message to display.</param>
    public void WriteSuccess(string text)
    {
        WriteLine(text, ConsoleColor.Green);
    }
}