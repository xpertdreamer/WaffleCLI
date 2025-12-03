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
        private DateTime _lastCursorBlink = DateTime.Now;
        private bool _cursorVisible = true;

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
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.Gray);
        public ColorScheme PlaceholderColors { get; set; } = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.Black);

        public TextBox(string id) : base(id)
        {
            Width = 20;
            Height = 1;
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            int absoluteX = AbsoluteX;
            int absoluteY = AbsoluteY;
            
            var colors = HasFocus ? FocusColors : NormalColors;
            var borderStyle = HasFocus ? BorderStyle.Double : BorderStyle.Single;
            
            renderEngine.FillRectangle(absoluteX, absoluteY, Width, Height, ' ', colors);
            renderEngine.DrawBox(absoluteX, absoluteY, Width, Height, borderStyle, colors);

            int maxDisplayLength = Math.Max(0, Width - 2);
            if (maxDisplayLength <= 0)
            {
                base.Render(renderEngine);
                return;
            }

            string displayText = string.IsNullOrEmpty(Text) ? Placeholder : Text;
            var displayColors = string.IsNullOrEmpty(Text) ? PlaceholderColors : colors;

            int displayStart = 0;
            if (displayText.Length > maxDisplayLength)
            {
                if (HasFocus)
                {
                    displayStart = Math.Max(0, _cursorPosition - maxDisplayLength + 1);
                    displayStart = Math.Min(displayStart, displayText.Length - maxDisplayLength);
                }
                else
                {
                    displayStart = 0;
                }
                displayText = displayText.Substring(displayStart, Math.Min(maxDisplayLength, displayText.Length - displayStart));
            }

            if (!string.IsNullOrEmpty(displayText))
            {
                renderEngine.DrawString(absoluteX + 1, absoluteY, displayText, displayColors);
            }

            if (HasFocus && IsEnabled)
            {
                if ((DateTime.Now - _lastCursorBlink).TotalMilliseconds > 500)
                {
                    _cursorVisible = !_cursorVisible;
                    _lastCursorBlink = DateTime.Now;
                }

                if (_cursorVisible)
                {
                    int cursorDisplayPos = _cursorPosition - displayStart;
                    if (cursorDisplayPos >= 0 && cursorDisplayPos < maxDisplayLength)
                    {
                        int cursorX = absoluteX + 1 + cursorDisplayPos;
                        var cursorColors = new ColorScheme(colors.Background, colors.Foreground);
                        char cursorChar = cursorDisplayPos < displayText.Length ? displayText[cursorDisplayPos] : ' ';
                        renderEngine.DrawChar(cursorX, absoluteY, cursorChar, cursorColors);
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
                        Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: Backspace, text: '{_text}'");
                    }
                    return true;

                case ConsoleKey.Delete:
                    if (_cursorPosition < _text.Length)
                    {
                        _text = _text.Remove(_cursorPosition, 1);
                        Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: Delete, text: '{_text}'");
                    }
                    return true;

                case ConsoleKey.LeftArrow:
                    if (_cursorPosition > 0)
                    {
                        _cursorPosition--;
                        Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: Move left, position: {_cursorPosition}");
                    }
                    return true;

                case ConsoleKey.RightArrow:
                    if (_cursorPosition < _text.Length)
                    {
                        _cursorPosition++;
                        Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: Move right, position: {_cursorPosition}");
                    }
                    return true;

                case ConsoleKey.Home:
                    _cursorPosition = 0;
                    Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: Home, position: {_cursorPosition}");
                    return true;

                case ConsoleKey.End:
                    _cursorPosition = _text.Length;
                    Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: End, position: {_cursorPosition}");
                    return true;

                default:
                    // Handle printable characters
                    if (!char.IsControl(inputEvent.Character) && 
                        inputEvent.Character >= 32 && 
                        _text.Length < MaxLength)
                    {
                        _text = _text.Insert(_cursorPosition, inputEvent.Character.ToString());
                        _cursorPosition++;
                        Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id}: Added char '{inputEvent.Character}', text: '{_text}'");
                        return true;
                    }
                    break;
            }

            return false;
        }

        public override void OnFocus()
        {
            base.OnFocus();
            _cursorVisible = true;
            _lastCursorBlink = DateTime.Now;
            Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id} received focus");
        }

        public override void OnBlur()
        {
            base.OnBlur();
            Infrastructure.Logging.TuiLogger.LogDebug($"TextBox {Id} lost focus");
        }
    }
}