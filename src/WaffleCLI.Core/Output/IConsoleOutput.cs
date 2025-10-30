using System.Drawing;

namespace WaffleCLI.Core.Output;

/// <summary>
/// Provides an abstraction for console output operations with support for colored text and different message types.
/// </summary>
public interface IConsoleOutput
{
    /// <summary>
    /// Writes the specified text to the console without a line terminator.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    void Write(string text);
    
    /// <summary>
    /// Writes the specified text to the console without a line terminator.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    /// <param name="color">The foreground color to use for the text.</param>
    void Write(string text, ConsoleColor color);
    
    /// <summary>
    /// Writes the specified text to the console followed by a line terminator.
    /// </summary>
    /// <param name="text">The text to write to the console. If empty, writes just a line terminator.</param>
    void WriteLine(string text = "");
    
    /// <summary>
    /// Writes the specified text to the console with the specified foreground color, followed by a line terminator.
    /// </summary>
    /// <param name="text">The text to write to the console.</param>
    /// <param name="color">The foreground color to use for the text.</param>
    void WriteLine(string text, ConsoleColor color);
    
    /// <summary>
    /// Writes an error message to the console, typically using a distinctive color scheme.
    /// </summary>
    /// <param name="text">The error message to display.</param>
    void WriteError(string text);
    
    /// <summary>
    /// Writes a warning message to the console, typically using a distinctive color scheme.
    /// </summary>
    /// <param name="text">The warning message to display.</param>
    void WriteWarning(string text);
    
    /// <summary>
    /// Writes a success message to the console, typically using a distinctive color scheme.
    /// </summary>
    /// <param name="text">The success message to display.</param>
    void WriteSuccess(string text);
    
    /// <summary>
    /// Writes an informational message to the console, typically using a distinctive color scheme.
    /// </summary>
    /// <param name="text">The informational message to display.</param>
    void WriteInfo(string text);
}