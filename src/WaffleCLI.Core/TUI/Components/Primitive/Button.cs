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

            // Use absolute coordinates for rendering
            int absX = AbsoluteX;
            int absY = AbsoluteY;
    
            var colors = GetCurrentColors();
            var borderStyle = GetBorderStyle();
    
            // Draw background and border
            renderEngine.FillRectangle(absX, absY, Width, Height, ' ', colors);
            renderEngine.DrawBox(absX, absY, Width, Height, borderStyle, colors);
    
            // Draw text if available and there's enough space
            if (!string.IsNullOrEmpty(Text) && Width > 2 && Height > 0)
            {
                // Maximum text width (minus border)
                int maxTextWidth = Math.Max(0, Width - 2);
                string displayText = Text;
        
                // Trim text if it's too long
                if (displayText.Length > maxTextWidth)
                {
                    displayText = displayText.Substring(0, maxTextWidth);
                }
        
                // Calculate text position centered
                int textX = absX + 1; // Offset from left border
                int textY = absY + Height / 2; // Vertically centered
        
                // Center alignment
                if (displayText.Length < maxTextWidth)
                {
                    textX += (maxTextWidth - displayText.Length) / 2;
                }
        
                // Verify text fits within bounds
                if (textX >= absX && textX + displayText.Length <= absX + Width &&
                    textY >= absY && textY < absY + Height)
                {
                    renderEngine.DrawString(textX, textY, displayText, colors);
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
    
            // Force immediate visual feedback
            RequestVisualUpdate();
            
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} invoking OnClick action");
            OnClick?.Invoke();
    
            _isPressed = false;
    
            // Force another update after click completes
            RequestVisualUpdate();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            RequestVisualUpdate(); // Force visual update on focus
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} received focus");
        }

        public override void OnBlur()
        {
            base.OnBlur();
            _isPressed = false;
            RequestVisualUpdate(); // Force visual update on blur
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