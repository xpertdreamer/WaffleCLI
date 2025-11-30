using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Exceptions;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// Main render engine with double buffering and cross-platform support
    /// </summary>
    public class RenderEngine : IRenderEngine
    {
        private DoubleBuffer? _buffer;
        private bool _initialized = false;
        private int _viewportX, _viewportY, _viewportWidth, _viewportHeight;
        private bool _viewportActive = false;
        private ColorScheme _clearColors = ColorScheme.Default;

        public int Width => _buffer?.Width ?? 0;
        public int Height => _buffer?.Height ?? 0;

        public void Initialize(int width, int height)
        {
            TuiLogger.LogInfo($"Initializing RenderEngine with dimensions: {width}x{height}");
            
            if (width <= 0 || height <= 0)
            {
                TuiLogger.LogError($"Invalid buffer dimensions: {width}x{height}");
                throw new ArgumentException("Invalid buffer dimensions");
            }

            try
            {
                _buffer = new DoubleBuffer(width, height);
                _initialized = true;
                TuiLogger.LogInfo("RenderEngine initialized successfully");
                
                SetupConsole();
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Failed to initialize RenderEngine", ex);
                throw new TuiException("Failed to initialize RenderEngine", ex);
            }
        }

        public void BeginFrame()
        {
            if (!_initialized || _buffer == null)
            {
                TuiLogger.LogError("RenderEngine not initialized in BeginFrame");
                throw new TuiException("RenderEngine not initialized");
            }

            // Clear the back buffer at the start of each frame
            _buffer.Clear(_clearColors);
        }

        public void EndFrame()
        {
            if (!_initialized || _buffer == null) 
            {
                TuiLogger.LogError("RenderEngine not initialized in EndFrame");
                return;
            }
            
            try
            {
                _buffer.Swap();
                _buffer.RenderToConsole();
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Error in EndFrame", ex);
            }
        }

        public void DrawString(int x, int y, string text, ColorScheme colors)
        {
            if (!_initialized || string.IsNullOrEmpty(text) || _buffer == null) 
            {
                return;
            }

            for (int i = 0; i < text.Length; i++)
            {
                DrawChar(x + i, y, text[i], colors);
            }
        }

        public void DrawChar(int x, int y, char character, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            int drawX = _viewportActive ? x + _viewportX : x;
            int drawY = _viewportActive ? y + _viewportY : y;

            // Only draw if within buffer bounds and viewport
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

            // Draw corners
            DrawChar(x, y, borderChars.Value.TopLeft, colors);
            DrawChar(x + width - 1, y, borderChars.Value.TopRight, colors);
            DrawChar(x, y + height - 1, borderChars.Value.BottomLeft, colors);
            DrawChar(x + width - 1, y + height - 1, borderChars.Value.BottomRight, colors);

            // Draw horizontal lines
            for (int i = 1; i < width - 1; i++)
            {
                DrawChar(x + i, y, borderChars.Value.Horizontal, colors);
                DrawChar(x + i, y + height - 1, borderChars.Value.Horizontal, colors);
            }

            // Draw vertical lines
            for (int i = 1; i < height - 1; i++)
            {
                DrawChar(x, y + i, borderChars.Value.Vertical, colors);
                DrawChar(x + width - 1, y + i, borderChars.Value.Vertical, colors);
            }
        }

        public void DrawLine(int x1, int y1, int x2, int y2, char lineChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            // Bresenham's line algorithm
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
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        public void FillRectangle(int x, int y, int width, int height, char fillChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            for (int row = y; row < y + height && row < Height; row++)
            {
                for (int col = x; col < x + width && col < Width; col++)
                {
                    DrawChar(col, row, fillChar, colors);
                }
            }
        }

        public void Clear(ColorScheme colors)
        {
            if (!_initialized || _buffer == null) 
            {
                return;
            }
            
            _clearColors = colors;
            _buffer.Clear(colors);
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
                
                // Clear console and set initial position
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                
                TuiLogger.LogInfo("Console setup completed");
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Failed to setup console", ex);
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