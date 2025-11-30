using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// High-performance render engine with smart update strategies
    /// </summary>
    public class RenderEngine : IRenderEngine
    {
        private DoubleBuffer? _buffer;
        private bool _initialized = false;
        private int _viewportX, _viewportY, _viewportWidth, _viewportHeight;
        private bool _viewportActive = false;
        private ColorScheme _clearColors = ColorScheme.Default;
        private bool _requiresFullRender = true;
        private DateTime _lastRenderTime = DateTime.Now;
        private const int MIN_RENDER_INTERVAL_MS = 16; // ~60 FPS max

        public int Width => _buffer?.Width ?? 0;
        public int Height => _buffer?.Height ?? 0;

        public void Initialize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Invalid buffer dimensions");

            _buffer = new DoubleBuffer(width, height);
            _initialized = true;
            _requiresFullRender = true;
            
            SetupConsole();
        }

        public void BeginFrame()
        {
            if (!_initialized || _buffer == null)
                return;

            _buffer.Clear(_clearColors);
        }

        public void EndFrame()
        {
            if (!_initialized || _buffer == null) 
                return;
            
            // Throttle rendering to prevent excessive CPU usage
            var now = DateTime.Now;
            if ((now - _lastRenderTime).TotalMilliseconds < MIN_RENDER_INTERVAL_MS)
                return;
            
            try
            {
                _buffer.Swap();
                _buffer.RenderToConsole(_requiresFullRender);
                _requiresFullRender = false;
                _lastRenderTime = now;
            }
            catch (Exception ex)
            {
                _requiresFullRender = true; // Force full render on error
            }
        }

        public void DrawString(int x, int y, string text, ColorScheme colors)
        {
            if (!_initialized || string.IsNullOrEmpty(text) || _buffer == null) 
                return;

            // Optimized: draw entire string at once if possible
            for (int i = 0; i < text.Length; i++)
            {
                int drawX = _viewportActive ? x + i + _viewportX : x + i;
                int drawY = _viewportActive ? y + _viewportY : y;
                
                if (drawX >= 0 && drawX < Width && drawY >= 0 && drawY < Height)
                {
                    if (!_viewportActive || IsInViewport(x + i, y))
                    {
                        _buffer.SetPixel(drawX, drawY, text[i], colors.Foreground, colors.Background);
                    }
                }
            }
        }

        public void DrawChar(int x, int y, char character, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            int drawX = _viewportActive ? x + _viewportX : x;
            int drawY = _viewportActive ? y + _viewportY : y;

            if (drawX >= 0 && drawX < Width && drawY >= 0 && drawY < Height)
            {
                if (!_viewportActive || IsInViewport(x, y))
                {
                    _buffer.SetPixel(drawX, drawY, character, colors.Foreground, colors.Background);
                }
            }
        }

        public void DrawBox(int x, int y, int width, int height, BorderStyle border, ColorScheme colors)
        {
            if (!_initialized || _buffer == null || width <= 0 || height <= 0) return;

            var borderChars = GetBorderChars(border);
            if (borderChars == null) return;

            // Optimized box drawing - only draw visible parts
            DrawChar(x, y, borderChars.Value.TopLeft, colors);
            DrawChar(x + width - 1, y, borderChars.Value.TopRight, colors);
            DrawChar(x, y + height - 1, borderChars.Value.BottomLeft, colors);
            DrawChar(x + width - 1, y + height - 1, borderChars.Value.BottomRight, colors);

            // Horizontal lines
            for (int i = 1; i < width - 1; i++)
            {
                DrawChar(x + i, y, borderChars.Value.Horizontal, colors);
                DrawChar(x + i, y + height - 1, borderChars.Value.Horizontal, colors);
            }

            // Vertical lines
            for (int i = 1; i < height - 1; i++)
            {
                DrawChar(x, y + i, borderChars.Value.Vertical, colors);
                DrawChar(x + width - 1, y + i, borderChars.Value.Vertical, colors);
            }
        }

        public void DrawLine(int x1, int y1, int x2, int y2, char lineChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            // Bresenham's line algorithm (optimized)
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawChar(x1, y1, lineChar, colors);
                if (x1 == x2 && y1 == y2) break;
                
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x1 += sx; }
                if (e2 < dx) { err += dx; y1 += sy; }
            }
        }

        public void FillRectangle(int x, int y, int width, int height, char fillChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            // Optimized: only fill visible area
            int endX = Math.Min(x + width, Width);
            int endY = Math.Min(y + height, Height);
            
            for (int row = Math.Max(y, 0); row < endY; row++)
            {
                for (int col = Math.Max(x, 0); col < endX; col++)
                {
                    DrawChar(col, row, fillChar, colors);
                }
            }
        }

        public void Clear(ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;
            
            _clearColors = colors;
            _requiresFullRender = true;
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            _viewportX = Math.Max(0, x);
            _viewportY = Math.Max(0, y);
            _viewportWidth = Math.Max(0, width);
            _viewportHeight = Math.Max(0, height);
            _viewportActive = true;
            _requiresFullRender = true;
        }

        public void ResetViewport()
        {
            _viewportActive = false;
            _requiresFullRender = true;
        }

        public void RequestFullRedraw()
        {
            _requiresFullRender = true;
        }

        private bool IsInViewport(int x, int y)
        {
            return x >= _viewportX && x < _viewportX + _viewportWidth &&
                   y >= _viewportY && y < _viewportY + _viewportHeight;
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
                Console.Clear();
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