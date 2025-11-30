using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Label component for displaying text
    /// </summary>
    public class Label : ComponentBase, ILabel
    {
        private string _text = string.Empty;

        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                // Auto-size if width is 0
                if (Width == 0)
                {
                    Width = _text.Length;
                }
            }
        }

        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
        public ColorScheme Colors { get; set; } = ColorScheme.Default;

        public Label(string id) : base(id)
        {
            Height = 1;
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible || string.IsNullOrEmpty(Text)) return;

            string displayText = Text;
            if (displayText.Length > Width)
            {
                displayText = displayText.Substring(0, Width);
            }

            int textX = CalculateTextX(displayText);
            renderEngine.DrawString(textX, Y, displayText, Colors);

            base.Render(renderEngine);
        }

        private int CalculateTextX(string text)
        {
            return TextAlignment switch
            {
                TextAlignment.Center => X + (Width - text.Length) / 2,
                TextAlignment.Right => X + Width - text.Length,
                _ => X // Left
            };
        }
    }
}