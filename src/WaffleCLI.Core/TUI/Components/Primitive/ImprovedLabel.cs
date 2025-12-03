using System.ComponentModel;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Core.TUI.Components.Base;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Improved Label component with boundary-aware text rendering
    /// </summary>
    public class ImprovedLabel : ComponentBase, ILabel
    {
        private string _text = string.Empty;
        private string _cachedText = string.Empty;
        private string[] _cachedLines = Array.Empty<string>();
        private bool _textChanged = true;
        
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value ?? string.Empty;
                    _textChanged = true;
                    Infrastructure.Logging.TuiLogger.LogDebug($"Label {Id} text changed");
                }
            }
        }

        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;
        public ColorScheme Colors { get; set; } = ColorScheme.Default;

        public ImprovedLabel(string id) : base(id)
        {
            Height = 1;
        }

        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;

            // Update cache if text changed
            if (_textChanged)
            {
                UpdateTextCache();
                _textChanged = false;
            }

            if (_cachedLines.Length == 0) return;

            // Use absolute coordinates for rendering
            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            // Draw only lines that fit in component height
            int linesToDraw = Math.Min(_cachedLines.Length, Height);
            
            for (int i = 0; i < linesToDraw; i++)
            {
                string line = _cachedLines[i];
                int textY = absY + i;
                
                // Skip if Y coordinate is out of bounds
                if (textY < absY || textY >= absY + Height) continue;
                
                // Calculate X position based on alignment
                int textX = CalculateTextX(line, absX);
                
                // Ensure text doesn't exceed component bounds
                if (textX < absX) textX = absX;
                if (textX + line.Length > absX + Width)
                {
                    // Trim line if it's too long
                    line = line.Substring(0, Math.Max(0, (absX + Width) - textX));
                }
                
                // Draw the line if it's not empty
                if (line.Length > 0)
                {
                    renderEngine.DrawString(textX, textY, line, Colors);
                }
            }

            base.Render(renderEngine);
        }

        private void UpdateTextCache()
        {
            _cachedText = _text;
            
            // Split text into lines
            var lines = _text.Replace("\r\n", "\n")
                            .Replace("\r", "\n")
                            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            // Process each line - trim if too long
            List<string> processedLines = new List<string>();
            foreach (var line in lines)
            {
                if (line.Length > Width)
                {
                    // Split too long lines into multiple lines
                    for (int i = 0; i < line.Length; i += Width)
                    {
                        string chunk = line.Substring(i, Math.Min(Width, line.Length - i));
                        processedLines.Add(chunk);
                        
                        if (processedLines.Count >= Height)
                            break;
                    }
                }
                else
                {
                    processedLines.Add(line);
                }
                
                if (processedLines.Count >= Height)
                    break;
            }
            
            _cachedLines = processedLines.ToArray();
            
            // Auto-adjust height if not set
            if (Height == 1 && _cachedLines.Length > 1)
            {
                Height = _cachedLines.Length;
                Infrastructure.Logging.TuiLogger.LogDebug($"Label {Id} auto-adjusted height to {Height}");
            }
        }

        private int CalculateTextX(string line, int baseX)
        {
            return TextAlignment switch
            {
                TextAlignment.Center => baseX + Math.Max(0, (Width - line.Length) / 2),
                TextAlignment.Right => baseX + Math.Max(0, Width - line.Length),
                _ => baseX // Left
            };
        }
        
        /// <summary>
        /// Forces text update on next render
        /// </summary>
        public void InvalidateText()
        {
            _textChanged = true;
        }
    }
}