using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Fixed Button component with proper input handling
    /// </summary>
    public class Button : FocusableComponentBase, IButton
    {
        private string _text = string.Empty;
        private bool _isPressed = false;

        public string Text 
        { 
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                if (_text.Length > Width - 4)
                    _text = _text.Substring(0, Width - 4);
            }
        }

        public Action? OnClick { get; set; }
        public ColorScheme NormalColors { get; set; } = ColorScheme.Primary;
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.White);
        public ColorScheme PressedColors { get; set; } = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkRed);
        public ColorScheme DisabledColors { get; set; } = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.Black);

        public Button(string id) : base(id)
        {
            Width = 12;
            Height = 3;
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            var colors = GetCurrentColors();
            var borderStyle = GetBorderStyle();
            
            // Draw button background
            renderEngine.FillRectangle(X, Y, Width, Height, ' ', colors);
            
            // Draw button border
            renderEngine.DrawBox(X, Y, Width, Height, borderStyle, colors);
            
            // Draw text centered
            if (!string.IsNullOrEmpty(Text) && Width >= 4 && Height >= 1)
            {
                int textX = X + Math.Max(1, (Width - Text.Length) / 2);
                int textY = Y + Math.Max(0, Height / 2);
                
                if (textX >= X && textX + Text.Length <= X + Width && textY >= Y && textY < Y + Height)
                {
                    renderEngine.DrawString(textX, textY, Text, colors);
                }
            }

            base.Render(renderEngine);
        }

        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) 
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Button {Id} is disabled, ignoring input");
                return false;
            }

            Infrastructure.Logging.TuiLogger.LogDebug($"Button {Id} received input: {inputEvent.Key}");

            // First try common navigation (Tab, Enter, Escape)
            if (HandleCommonNavigation(inputEvent))
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Button {Id} handled common navigation");
                return true;
            }

            // Handle button-specific input
            switch (inputEvent.Key)
            {
                case ConsoleKey.Spacebar:
                    Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} pressed via Spacebar");
                    PressButton();
                    return true;
                    
                case ConsoleKey.Enter:
                    Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} pressed via Enter");
                    PressButton();
                    return true;
            }

            return false;
        }

        private void PressButton()
        {
            _isPressed = true;
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} invoking OnClick action");
            OnClick?.Invoke();
            _isPressed = false;
        }

        public override void OnFocus()
        {
            base.OnFocus();
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} received focus");
        }

        public override void OnBlur()
        {
            base.OnBlur();
            _isPressed = false;
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} lost focus");
        }

        private ColorScheme GetCurrentColors()
        {
            if (!IsEnabled) return DisabledColors;
            if (_isPressed) return PressedColors;
            return HasFocus ? FocusColors : NormalColors;
        }
        
        private BorderStyle GetBorderStyle()
        {
            return HasFocus ? BorderStyle.Double : BorderStyle.Single;
        }
        
        protected override bool HandleConfirm()
        {
            if (IsEnabled && OnClick != null)
            {
                Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} confirmed via HandleConfirm");
                PressButton();
                return true;
            }
            return false;
        }
    }
}