using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// Optimized render engine with performance improvements
    /// </summary>
    public class RenderEngine : IRenderEngine
    {
        private DoubleBuffer? _buffer;
        private bool _initialized = false;
        private int _viewportX, _viewportY, _viewportWidth, _viewportHeight;
        private bool _viewportActive = false;
        private ColorScheme _clearColors = ColorScheme.Default;
        private DateTime _lastRenderTime = DateTime.Now;
        private const int RENDER_THROTTLE_MS = 8; // ~120 FPS max

        public int Width => _buffer?.Width ?? 0;
        public int Height => _buffer?.Height ?? 0;

        public void Initialize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Invalid buffer dimensions");

            _buffer = new DoubleBuffer(width, height);
            _initialized = true;
            
            SetupConsole();
        }

        public void BeginFrame()
        {
            if (!_initialized || _buffer == null) return;
            _buffer.Clear(_clearColors);
        }

        public void EndFrame()
        {
            if (!_initialized || _buffer == null) return;
            
            // Throttle rendering to prevent excessive updates
            var now = DateTime.Now;
            if ((now - _lastRenderTime).TotalMilliseconds < RENDER_THROTTLE_MS)
                return;
            
            _buffer.Swap();
            _buffer.RenderToConsole();
            _lastRenderTime = now;
        }

        public void DrawString(int x, int y, string text, ColorScheme colors)
        {
            if (!_initialized || string.IsNullOrEmpty(text) || _buffer == null) 
                return;

            // Clamp coordinates to buffer bounds
            int startX = Math.Max(0, x);
            int endX = Math.Min(Width, x + text.Length);
            int drawY = Math.Clamp(y, 0, Height - 1);
            
            if (drawY < 0 || drawY >= Height) return;
            if (startX >= endX) return;

            int textStart = Math.Max(0, -x);
            int textLength = Math.Min(text.Length, endX - startX);
            
            if (textLength <= 0) return;

            string visibleText = text.Substring(textStart, textLength);
            
            for (int i = 0; i < visibleText.Length; i++)
            {
                _buffer.SetPixel(startX + i, drawY, visibleText[i], colors.Foreground, colors.Background);
            }
        }

        public void DrawChar(int x, int y, char character, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;
            
            // Clamp coordinates to buffer bounds
            int clampedX = Math.Clamp(x, 0, Width - 1);
            int clampedY = Math.Clamp(y, 0, Height - 1);
            
            _buffer.SetPixel(clampedX, clampedY, character, colors.Foreground, colors.Background);
        }

        public void DrawBox(int x, int y, int width, int height, BorderStyle border, ColorScheme colors)
        {
            if (!_initialized || _buffer == null || width <= 0 || height <= 0) return;

            var borderChars = GetBorderChars(border);
            if (borderChars == null) return;

            // Calculate visible area
            int visibleX1 = Math.Max(0, x);
            int visibleY1 = Math.Max(0, y);
            int visibleX2 = Math.Min(Width - 1, x + width - 1);
            int visibleY2 = Math.Min(Height - 1, y + height - 1);

            // Only draw visible corners
            if (visibleX1 <= visibleX2 && visibleY1 <= visibleY2)
            {
                // Corners
                if (visibleX1 == x && visibleY1 == y)
                    DrawChar(x, y, borderChars.Value.TopLeft, colors);
                if (visibleX2 == x + width - 1 && visibleY1 == y)
                    DrawChar(x + width - 1, y, borderChars.Value.TopRight, colors);
                if (visibleX1 == x && visibleY2 == y + height - 1)
                    DrawChar(x, y + height - 1, borderChars.Value.BottomLeft, colors);
                if (visibleX2 == x + width - 1 && visibleY2 == y + height - 1)
                    DrawChar(x + width - 1, y + height - 1, borderChars.Value.BottomRight, colors);

                // Horizontal lines (only visible portions)
                for (int i = visibleX1 + 1; i < visibleX2; i++)
                {
                    if (i > x && i < x + width - 1)
                    {
                        if (visibleY1 == y)
                            DrawChar(i, y, borderChars.Value.Horizontal, colors);
                        if (visibleY2 == y + height - 1)
                            DrawChar(i, y + height - 1, borderChars.Value.Horizontal, colors);
                    }
                }

                // Vertical lines (only visible portions)
                for (int i = visibleY1 + 1; i < visibleY2; i++)
                {
                    if (i > y && i < y + height - 1)
                    {
                        if (visibleX1 == x)
                            DrawChar(x, i, borderChars.Value.Vertical, colors);
                        if (visibleX2 == x + width - 1)
                            DrawChar(x + width - 1, i, borderChars.Value.Vertical, colors);
                    }
                }
            }
        }

        public void DrawLine(int x1, int y1, int x2, int y2, char lineChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            // Bresenham's line algorithm (clipped to visible area)
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Only draw if within visible area
                if (x1 >= 0 && x1 < Width && y1 >= 0 && y1 < Height)
                {
                    DrawChar(x1, y1, lineChar, colors);
                }

                if (x1 == x2 && y1 == y2) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x1 += sx; }
                if (e2 < dx) { err += dx; y1 += sy; }
            }
        }

        public void FillRectangle(int x, int y, int width, int height, char fillChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            // Calculate visible area
            int startX = Math.Max(0, x);
            int startY = Math.Max(0, y);
            int endX = Math.Min(Width, x + width);
            int endY = Math.Min(Height, y + height);

            // Only fill visible portion
            for (int row = startY; row < endY; row++)
            {
                for (int col = startX; col < endX; col++)
                {
                    DrawChar(col, row, fillChar, colors);
                }
            }
        }

        public void Clear(ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;
            _clearColors = colors;
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            _viewportX = Math.Max(0, x);
            _viewportY = Math.Max(0, y);
            _viewportWidth = Math.Max(0, width);
            _viewportHeight = Math.Max(0, height);
            _viewportActive = true;
        }

        public void ResetViewport()
        {
            _viewportActive = false;
        }

        private BorderChars? GetBorderChars(BorderStyle style)
        {
            return style switch
            {
                BorderStyle.Single => new BorderChars('┌', '┐', '└', '┘', '─', '│'),
                BorderStyle.Double => new BorderChars('╔', '╗', '╚', '╝', '═', '║'),
                BorderStyle.Rounded => new BorderChars('╭', '╮', '╰', '╯', '─', '│'),
                BorderStyle.Thick => new BorderChars('┏', '┓', '┗', '┛', '━', '┃'),
                _ => null
            };
        }

        private void SetupConsole()
        {
            try
            {
                Console.CursorVisible = false;
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.Clear();
                Console.SetCursorPosition(0, 0);
            }
            catch (Exception ex)
            {
                throw new TuiException("Failed to setup console", ex);
            }
        }

        public void Dispose()
        {
            _buffer?.Dispose();
            _initialized = false;
            
            try
            {
                Console.ResetColor();
                Console.CursorVisible = true;
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    internal struct BorderChars
    {
        public char TopLeft { get; }
        public char TopRight { get; }
        public char BottomLeft { get; }
        public char BottomRight { get; }
        public char Horizontal { get; }
        public char Vertical { get; }

        public BorderChars(char topLeft, char topRight, char bottomLeft, char bottomRight, char horizontal, char vertical)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Horizontal = horizontal;
            Vertical = vertical;
        }
    }
}