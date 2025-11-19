using WaffleCLI.Core.TUI;

namespace WaffleCLI.Runtime.TUI.Elements;

/// <summary>
/// Represents a text view element for displaying and scrolling through multi-line text content in a TUI.
/// </summary>
/// <remarks>
/// Provides scrollable text display with keyboard navigation support including arrow keys,
/// page up/down, and home/end keys for efficient text browsing.
/// </remarks>
public class TuiTextView : TuiElement
{
    private readonly List<string> _lines = [];
    private int _scrollOffset = 0;

    /// <summary>
    /// Gets or sets the text content of the text view.
    /// </summary>
    /// <remarks>
    /// Setting this property replaces all existing content and splits the text into lines based on newline characters.
    /// Automatically adjusts the scroll offset to display the most recent content.
    /// </remarks>
    public string Text
    {
        get => string.Join(Environment.NewLine, _lines);
        set
        {
            _lines.Clear();
            if (!string.IsNullOrEmpty(value))
                _lines.AddRange(value.Split('\n'));
        }
    }

    /// <summary>
    /// Appends a line of text to the text view.
    /// </summary>
    /// <param name="line">The line of text to append.</param>
    /// <remarks>
    /// Automatically adjusts the scroll offset to ensure the newly added line is visible when appropriate.
    /// </remarks>
    public void AppendLine(string line)
    {
        _lines.Add(line);
        _scrollOffset = Math.Max(0, _lines.Count - (Height - 2));
    }

    /// <summary>
    /// Clears all text content from the text view.
    /// </summary>
    /// <remarks>
    /// Resets both the content and scroll position to their initial states.
    /// </remarks>
    public void Clear()
    {
        _lines.Clear();
        _scrollOffset = 0;
    }

    /// <summary>
    /// Renders the text view element to the console.
    /// </summary>
    /// <remarks>
    /// Draws a bordered box and displays the text content with proper scrolling and text truncation.
    /// Preserves the original console colors and restores them after rendering.
    /// </remarks>
    public override void Render()
    {
        var originalBackgroundColor = Console.BackgroundColor;
        var originalTextColor = Console.ForegroundColor;
        
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        
        DrawBox(0, 0, Width, Height);
        
        int linesToShow = Math.Min(Height - 2, _lines.Count - _scrollOffset);

        for (int i = 0; i < linesToShow; i++)
        {
            int lineIndex = _scrollOffset + i;
            SetCursorPosition(1, i + 1);
            
            var line = _lines[lineIndex];
            var displayText = line.Length > Width - 2 ? line[..(Width - 2)] : line.PadRight(Width - 2);
            Console.Write(displayText);
        }
        
        Console.BackgroundColor = originalBackgroundColor;
        Console.ForegroundColor = originalTextColor;
    }

    /// <summary>
    /// Handles keyboard input for text view navigation.
    /// </summary>
    /// <param name="keyInfo">The keyboard input information.</param>
    /// <returns>True if the key was handled by this element; otherwise, false.</returns>
    /// <remarks>
    /// Supports vertical scrolling with arrow keys, page navigation with PageUp/PageDown,
    /// and quick navigation to the beginning or end with Home/End keys.
    /// </remarks>
    public override bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                if (_scrollOffset > 0)
                {
                    _scrollOffset -= 1;
                    return true;
                }
                break;
            
            case ConsoleKey.DownArrow:
                if (_scrollOffset < _lines.Count - (Height - 2))
                {
                    _scrollOffset += 1;
                    return true;
                }
                break;
            
            case ConsoleKey.PageUp:
                _scrollOffset = Math.Max(0, _scrollOffset - (Height - 2));
                return true;
            
            case ConsoleKey.PageDown:
                _scrollOffset = Math.Min(_lines.Count - (Height - 2), _scrollOffset + (Height - 2));
                return true;
                
            case ConsoleKey.Home:
                _scrollOffset = 0;
                return true;
                
            case ConsoleKey.End:
                _scrollOffset = Math.Max(0, _lines.Count - (Height - 2));
                return true;
        }
        
        return false;
    }
}