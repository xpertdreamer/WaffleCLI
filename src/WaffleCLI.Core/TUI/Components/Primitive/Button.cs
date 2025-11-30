using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Button component
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
                // Truncate if too long (respecting borders)
                if (_text.Length > Width - 4)
                    _text = _text.Substring(0, Width - 4);
            }
        }

        public Action? OnClick { get; set; }
        public ColorScheme NormalColors { get; set; } = ColorScheme.Primary;
        public ColorScheme FocusColors { get; set; } = ColorScheme.Focus;
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
            
            // Draw button background
            renderEngine.FillRectangle(X, Y, Width, Height, ' ', colors);
            
            // Draw button border
            renderEngine.DrawBox(X, Y, Width, Height, GetBorderStyle(), colors);
            
            // Draw text centered (only if there's space)
            if (!string.IsNullOrEmpty(Text) && Width >= 4 && Height >= 1)
            {
                int textX = X + Math.Max(1, (Width - Text.Length) / 2);
                int textY = Y + Math.Max(0, Height / 2);
                
                // Ensure text doesn't overflow
                if (textX >= X && textX + Text.Length <= X + Width && textY >= Y && textY < Y + Height)
                {
                    renderEngine.DrawString(textX, textY, Text, colors);
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
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    _isPressed = true;
                    OnClick?.Invoke();
                    System.Threading.Thread.Sleep(100); // Visual feedback
                    _isPressed = false;
                    return true;
            }

            return false;
        }

        public override void OnFocus()
        {
            base.OnFocus();
        }

        public override void OnBlur()
        {
            base.OnBlur();
            _isPressed = false;
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
                _isPressed = true;
                OnClick();
                _isPressed = false;
                return true;
            }
            return false;
        }
    }
}