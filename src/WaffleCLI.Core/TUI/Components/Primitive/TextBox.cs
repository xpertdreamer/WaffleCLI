using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Text input component
    /// </summary>
    public class TextBox : FocusableComponentBase, ITextBox
    {
        private string _text = string.Empty;
        private int _cursorPosition = 0;
        private string _placeholder = string.Empty;

        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                _cursorPosition = Math.Min(_cursorPosition, _text.Length);
            }
        }

        public string Placeholder
        {
            get => _placeholder;
            set => _placeholder = value ?? string.Empty;
        }

        public int MaxLength { get; set; } = 256;
        public ColorScheme NormalColors { get; set; } = ColorScheme.Default;
        public ColorScheme FocusColors { get; set; } = ColorScheme.Focus;
        public ColorScheme PlaceholderColors { get; set; } = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.Black);

        public TextBox(string id) : base(id)
        {
            Width = 20;
            Height = 1;
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            var colors = HasFocus ? FocusColors : NormalColors;
            
            // Draw background
            renderEngine.FillRectangle(X, Y, Width, Height, ' ', colors);
            
            // Draw border
            renderEngine.DrawBox(X, Y, Width, Height, GetBorderStyle(), colors);

            // Calculate available space for text
            int maxDisplayLength = Math.Max(0, Width - 2);
            if (maxDisplayLength <= 0)
            {
                base.Render(renderEngine);
                return;
            }

            // Prepare display text
            string displayText = string.IsNullOrEmpty(Text) ? Placeholder : Text;
            var displayColors = string.IsNullOrEmpty(Text) ? PlaceholderColors : colors;

            // Handle text scrolling for long content
            int displayStart = 0;
            if (displayText.Length > maxDisplayLength)
            {
                if (HasFocus)
                {
                    // Scroll to show cursor position
                    displayStart = Math.Max(0, _cursorPosition - maxDisplayLength + 1);
                    displayStart = Math.Min(displayStart, displayText.Length - maxDisplayLength);
                }
                else
                {
                    // Show beginning of text when not focused
                    displayStart = 0;
                }
                displayText = displayText.Substring(displayStart, Math.Min(maxDisplayLength, displayText.Length - displayStart));
            }

            // Draw text
            if (!string.IsNullOrEmpty(displayText))
            {
                renderEngine.DrawString(X + 1, Y, displayText, displayColors);
            }

            // Draw cursor if focused and enabled
            if (HasFocus && IsEnabled)
            {
                int cursorDisplayPos = _cursorPosition - displayStart;
                if (cursorDisplayPos >= 0 && cursorDisplayPos < maxDisplayLength)
                {
                    int cursorX = X + 1 + cursorDisplayPos;
                    // Show blinking cursor (simple implementation)
                    if (DateTime.Now.Millisecond < 500)
                    {
                        renderEngine.DrawChar(cursorX, Y, '_', colors);
                    }
                }
            }

            base.Render(renderEngine);
        }

        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) return false;

            if (HandleCommonNavigation(inputEvent))
                return true;

            switch (inputEvent.Key)
            {
                case ConsoleKey.Backspace:
                    if (_cursorPosition > 0 && _text.Length > 0)
                    {
                        _text = _text.Remove(_cursorPosition - 1, 1);
                        _cursorPosition--;
                    }
                    return true;

                case ConsoleKey.Delete:
                    if (_cursorPosition < _text.Length)
                    {
                        _text = _text.Remove(_cursorPosition, 1);
                    }
                    return true;

                case ConsoleKey.LeftArrow:
                    if (_cursorPosition > 0)
                    {
                        _cursorPosition--;
                    }
                    return true;

                case ConsoleKey.RightArrow:
                    if (_cursorPosition < _text.Length)
                    {
                        _cursorPosition++;
                    }
                    return true;

                case ConsoleKey.Home:
                    _cursorPosition = 0;
                    return true;

                case ConsoleKey.End:
                    _cursorPosition = _text.Length;
                    return true;

                default:
                    // Handle printable characters
                    if (!char.IsControl(inputEvent.Character) && 
                        inputEvent.Character >= 32 && 
                        _text.Length < MaxLength)
                    {
                        _text = _text.Insert(_cursorPosition, inputEvent.Character.ToString());
                        _cursorPosition++;
                        return true;
                    }
                    break;
            }

            return false;
        }
        
        private BorderStyle GetBorderStyle()
        {
            return HasFocus ? BorderStyle.Double : BorderStyle.Single;
        }
    }
}