using WaffleCLI.Core.TUI;

namespace WaffleCLI.Runtime.TUI.Elements;

/// <summary>
/// Represents a text input field element for TUI applications with cursor navigation and text editing capabilities.
/// </summary>
/// <remarks>
/// Provides a single-line text input field with placeholder support, cursor navigation, text editing,
/// and scrolling for long text content. Supports focus management and text submission events.
/// </remarks>
public class TuiTextField : TuiElement
{
    private string _text = string.Empty;
    private int _cursorPosition = 0;
    private int _scrollOffset = 0;

    /// <summary>
    /// Occurs when the text field is submitted, typically by pressing the Enter key.
    /// </summary>
    public event Action<string>? TextSubmitted;

    /// <summary>
    /// Gets or sets the text content of the text field.
    /// </summary>
    /// <remarks>
    /// Setting this property automatically adjusts the cursor position and scroll offset to maintain proper display.
    /// </remarks>
    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            _cursorPosition = Math.Min(_cursorPosition, _text.Length);
            UpdateScrollOffset();
        }
    }
    
    /// <summary>
    /// Gets or sets the placeholder text displayed when the text field is empty.
    /// </summary>
    public string PlaceHolder { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the text field currently has input focus.
    /// </summary>
    /// <remarks>
    /// When focused, the text field displays with different colors and accepts keyboard input.
    /// </remarks>
    public bool HasFocus { get; set; }

    /// <summary>
    /// Renders the text field element to the console.
    /// </summary>
    /// <remarks>
    /// Draws a bordered box and displays the text content with proper scrolling and cursor positioning.
    /// Shows placeholder text when empty and not focused. Highlights the current cursor position when focused.
    /// Preserves the original console colors and restores them after rendering.
    /// </remarks>
    public override void Render()
    {
        var originalBackgroundColor = Console.BackgroundColor;
        var originalTextColor = Console.ForegroundColor;

        if (HasFocus)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
        }
        
        DrawBox(0, 0, Width, Height);
        
        SetCursorPosition(1, 1);

        string displayText;
        if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(PlaceHolder))
        {
            displayText = PlaceHolder.PadRight(Width - 2);
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }
        else
        {
            var visibleText = _text.Substring(_scrollOffset, Math.Min(Width - 2, _text.Length - _scrollOffset));
            displayText = visibleText.PadRight(Width - 2);
        }
        
        Console.Write(displayText);

        if (HasFocus)
        {
            int cursorX = _cursorPosition - _scrollOffset - 1;
            if (cursorX >= 1 && cursorX < Width - 1)
            {
                SetCursorPosition(cursorX, 1);
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(_cursorPosition < _text.Length ? _text[_cursorPosition].ToString() : " ");
            }
        }
        
        Console.BackgroundColor = originalBackgroundColor;
        Console.ForegroundColor = originalTextColor;
    }

    /// <summary>
    /// Handles keyboard input for text field navigation and editing.
    /// </summary>
    /// <param name="keyInfo">The keyboard input information.</param>
    /// <returns>True if the key was handled by this element; otherwise, false.</returns>
    /// <remarks>
    /// Supports cursor movement with arrow keys, text editing with Backspace and Delete keys,
    /// text insertion with character keys, and text submission with Enter key.
    /// Only processes input when the text field has focus.
    /// </remarks>
    public override bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        if (!HasFocus) return false;

        switch (keyInfo.Key)
        {
            case ConsoleKey.LeftArrow:
                if (_cursorPosition > 0)
                {
                    _cursorPosition -= 1;
                    UpdateScrollOffset();
                    return true;
                }
                break;
            
            case ConsoleKey.RightArrow:
                if (_cursorPosition < _text.Length)
                {
                    _cursorPosition += 1;
                    UpdateScrollOffset();
                    return true;
                }
                break;
            
            case ConsoleKey.Home:
                _cursorPosition = 0;
                _scrollOffset = 0;
                return true;
            
            case ConsoleKey.End:
                _cursorPosition = _text.Length;
                UpdateScrollOffset();
                return true;
            
            case ConsoleKey.Backspace:
                if (_cursorPosition > 0)
                {
                    _text = _text.Remove(_cursorPosition - 1, 1);
                    _cursorPosition -= 1;
                    UpdateScrollOffset();
                    return true;
                } 
                break;
            
            case ConsoleKey.Delete:
                if (_cursorPosition < _text.Length)
                {
                    _text = _text.Remove(_cursorPosition, 1);
                    UpdateScrollOffset();
                    return true;
                }
                break;
            
            case ConsoleKey.Enter:
                TextSubmitted?.Invoke(_text);
                return true;
            
            default:
                if (!char.IsControl(keyInfo.KeyChar))
                {
                    _text = _text.Insert(_cursorPosition, keyInfo.KeyChar.ToString());
                    _cursorPosition += 1;
                    UpdateScrollOffset();
                    return true;
                }
                break;
        }
        
        return false;
    }

    /// <summary>
    /// Updates the scroll offset to ensure the cursor remains visible within the text field.
    /// </summary>
    /// <remarks>
    /// Automatically adjusts the horizontal scroll position based on the current cursor position
    /// and the available display width to keep the cursor visible at all times.
    /// </remarks>
    private void UpdateScrollOffset()
    {
        if (_cursorPosition < _scrollOffset)
            _scrollOffset = _cursorPosition;
        else if (_cursorPosition >= _scrollOffset + (Width - 2))
            _scrollOffset = _cursorPosition - (Width - 2) + 1;
    }
}