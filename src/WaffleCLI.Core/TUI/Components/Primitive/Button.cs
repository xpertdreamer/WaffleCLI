using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Button component with improved text rendering
    /// </summary>
    public class Button : FocusableComponentBase, IButton
    {
        private string _text = string.Empty;
        private bool _isPressed = false;

        /// <summary>
        /// Gets or sets the button text
        /// </summary>
        public string Text 
        { 
            get => _text;
            set
            {
                _text = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the click action
        /// </summary>
        public Action? OnClick { get; set; }
        
        /// <summary>
        /// Gets or sets the normal color scheme
        /// </summary>
        public ColorScheme NormalColors { get; set; } = ColorScheme.Primary;
        
        /// <summary>
        /// Gets or sets the focus color scheme
        /// </summary>
        public ColorScheme FocusColors { get; set; } = new ColorScheme(ConsoleColor.Black, ConsoleColor.White);
        
        /// <summary>
        /// Gets or sets the pressed color scheme
        /// </summary>
        public ColorScheme PressedColors { get; set; } = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkRed);
        
        /// <summary>
        /// Gets or sets the disabled color scheme
        /// </summary>
        public ColorScheme DisabledColors { get; set; } = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.Black);

        /// <summary>
        /// Initializes a new Button
        /// </summary>
        public Button(string id) : base(id)
        {
            Width = 12;
            Height = 3;
        }

        /// <summary>
        /// Renders the button with boundary-aware text positioning
        /// </summary>
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
                // Calculate maximum text width (inside borders)
                int maxTextWidth = Math.Max(0, Width - 2);
                string displayText = Text;
                
                // Trim text if it's too long
                if (displayText.Length > maxTextWidth)
                {
                    displayText = displayText.Substring(0, maxTextWidth);
                }
                
                // Calculate Y position (vertically centered)
                int textY = absY + Height / 2;
                if (textY < absY || textY >= absY + Height)
                    return; // Y position out of bounds
                
                // Calculate X position (horizontally centered within available space)
                int availableSpace = maxTextWidth;
                int textX = absX + 1; // Start after left border
                
                if (displayText.Length < availableSpace)
                {
                    // Center the text within available space
                    int padding = (availableSpace - displayText.Length) / 2;
                    textX += padding;
                }
                
                // Ensure text doesn't exceed button bounds
                if (textX < absX + 1)
                    textX = absX + 1;
                
                if (textX + displayText.Length > absX + Width - 1)
                    displayText = displayText.Substring(0, (absX + Width - 1) - textX);
                
                // Draw the text if it fits
                if (displayText.Length > 0)
                {
                    renderEngine.DrawString(textX, textY, displayText, colors);
                }
            }

            base.Render(renderEngine);
        }

        /// <summary>
        /// Handles input events
        /// </summary>
        public override bool HandleInput(InputEvent inputEvent)
        {
            if (!IsEnabled) 
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Button {Id} is disabled, ignoring input");
                return false;
            }

            Infrastructure.Logging.TuiLogger.LogDebug($"Button {Id} received input: {inputEvent.Key}");

            if (HandleCommonNavigation(inputEvent))
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Button {Id} handled common navigation");
                return true;
            }

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
            RequestVisualUpdate();
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} invoking OnClick action");
            OnClick?.Invoke();
            _isPressed = false;
            RequestVisualUpdate();
        }

        /// <summary>
        /// Called when button receives focus
        /// </summary>
        public override void OnFocus()
        {
            base.OnFocus();
            RequestVisualUpdate();
            Infrastructure.Logging.TuiLogger.LogInfo($"Button {Id} received focus");
        }

        /// <summary>
        /// Called when button loses focus
        /// </summary>
        public override void OnBlur()
        {
            base.OnBlur();
            _isPressed = false;
            RequestVisualUpdate();
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
        
        /// <summary>
        /// Handles confirm action
        /// </summary>
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