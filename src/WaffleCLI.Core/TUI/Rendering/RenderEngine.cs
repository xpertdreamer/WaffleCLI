using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// Rendering engine for TUI applications
    /// </summary>
    public class RenderEngine : IRenderEngine
    {
        private DoubleBuffer? _buffer;
        private bool _initialized = false;
        private int _viewportX, _viewportY, _viewportWidth, _viewportHeight;
        private bool _viewportActive = false;
        private ColorScheme _clearColors = ColorScheme.Default;
        private DateTime _lastRenderTime = DateTime.Now;
        private const int RENDER_THROTTLE_MS = 8;

        /// <summary>
        /// Gets the buffer width
        /// </summary>
        public int Width => _buffer?.Width ?? 0;
        
        /// <summary>
        /// Gets the buffer height
        /// </summary>
        public int Height => _buffer?.Height ?? 0;

        /// <summary>
        /// Initializes the render engine
        /// </summary>
        public void Initialize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Invalid buffer dimensions");

            _buffer = new DoubleBuffer(width, height);
            _initialized = true;
            
            SetupConsole();
        }

        /// <summary>
        /// Begins a new frame
        /// </summary>
        public void BeginFrame()
        {
            if (!_initialized || _buffer == null) return;
            _buffer.Clear(_clearColors);
        }

        /// <summary>
        /// Ends the current frame
        /// </summary>
        public void EndFrame()
        {
            if (!_initialized || _buffer == null) return;
            
            var now = DateTime.Now;
            if ((now - _lastRenderTime).TotalMilliseconds < RENDER_THROTTLE_MS)
                return;
            
            _buffer.Swap();
            _buffer.RenderToConsole();
            _lastRenderTime = now;
        }

        /// <summary>
        /// Draws a string with boundary checking
        /// </summary>
        public void DrawString(int x, int y, string text, ColorScheme colors)
        {
            if (!_initialized || string.IsNullOrEmpty(text) || _buffer == null) 
                return;

            // Check if Y coordinate is within buffer bounds
            if (y < 0 || y >= Height) return;

            // Calculate visible portion of the string
            int startX = Math.Max(0, x);
            int endX = Math.Min(Width, x + text.Length);
            
            if (startX >= endX) return; // No visible part

            // Calculate which part of the text to display
            int textStart = Math.Max(0, -x);
            int textLength = Math.Min(text.Length, endX - startX);
            
            if (textLength <= 0) return;

            string visibleText = text.Substring(textStart, textLength);
            
            // Draw each character with boundary checking
            for (int i = 0; i < visibleText.Length; i++)
            {
                int drawX = startX + i;
                if (drawX >= 0 && drawX < Width)
                {
                    _buffer.SetPixel(drawX, y, visibleText[i], colors.Foreground, colors.Background);
                }
            }
        }

        /// <summary>
        /// Draws a single character with boundary checking
        /// </summary>
        public void DrawChar(int x, int y, char character, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;
            
            // Check if coordinates are within buffer bounds
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            
            _buffer.SetPixel(x, y, character, colors.Foreground, colors.Background);
        }

        /// <summary>
        /// Draws a box with boundary checking
        /// </summary>
        public void DrawBox(int x, int y, int width, int height, BorderStyle border, ColorScheme colors)
        {
            if (!_initialized || _buffer == null || width <= 0 || height <= 0) return;

            var borderChars = GetBorderChars(border);
            if (borderChars == null) return;

            // Clamp coordinates to buffer bounds
            int x1 = Math.Max(0, x);
            int y1 = Math.Max(0, y);
            int x2 = Math.Min(Width - 1, x + width - 1);
            int y2 = Math.Min(Height - 1, y + height - 1);
            
            int visibleWidth = x2 - x1 + 1;
            int visibleHeight = y2 - y1 + 1;
            
            if (visibleWidth < 2 || visibleHeight < 2) 
                return;

            // Draw corners only if they fit within original box area
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                DrawChar(x, y, borderChars.Value.TopLeft, colors);
            
            if (x + width - 1 >= 0 && x + width - 1 < Width && y >= 0 && y < Height)
                DrawChar(x + width - 1, y, borderChars.Value.TopRight, colors);
            
            if (x >= 0 && x < Width && y + height - 1 >= 0 && y + height - 1 < Height)
                DrawChar(x, y + height - 1, borderChars.Value.BottomLeft, colors);
            
            if (x + width - 1 >= 0 && x + width - 1 < Width && y + height - 1 >= 0 && y + height - 1 < Height)
                DrawChar(x + width - 1, y + height - 1, borderChars.Value.BottomRight, colors);

            // Draw horizontal borders with boundary checking
            if (width > 2)
            {
                for (int i = x + 1; i < x + width - 1; i++)
                {
                    if (i >= 0 && i < Width)
                    {
                        // Top border
                        if (y >= 0 && y < Height)
                            DrawChar(i, y, borderChars.Value.Horizontal, colors);
                        
                        // Bottom border
                        if (y + height - 1 >= 0 && y + height - 1 < Height)
                            DrawChar(i, y + height - 1, borderChars.Value.Horizontal, colors);
                    }
                }
            }

            // Draw vertical borders with boundary checking
            if (height > 2)
            {
                for (int i = y + 1; i < y + height - 1; i++)
                {
                    if (i >= 0 && i < Height)
                    {
                        // Left border
                        if (x >= 0 && x < Width)
                            DrawChar(x, i, borderChars.Value.Vertical, colors);
                        
                        // Right border
                        if (x + width - 1 >= 0 && x + width - 1 < Width)
                            DrawChar(x + width - 1, i, borderChars.Value.Vertical, colors);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a line with boundary checking
        /// </summary>
        public void DrawLine(int x1, int y1, int x2, int y2, char lineChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Draw only if within buffer bounds
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

        /// <summary>
        /// Fills a rectangle with boundary checking
        /// </summary>
        public void FillRectangle(int x, int y, int width, int height, char fillChar, ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;

            // Clamp to buffer bounds
            int startX = Math.Max(0, x);
            int startY = Math.Max(0, y);
            int endX = Math.Min(Width, x + width);
            int endY = Math.Min(Height, y + height);

            for (int row = startY; row < endY; row++)
            {
                for (int col = startX; col < endX; col++)
                {
                    DrawChar(col, row, fillChar, colors);
                }
            }
        }

        /// <summary>
        /// Clears the display
        /// </summary>
        public void Clear(ColorScheme colors)
        {
            if (!_initialized || _buffer == null) return;
            _clearColors = colors;
        }

        /// <summary>
        /// Sets a viewport for clipping
        /// </summary>
        public void SetViewport(int x, int y, int width, int height)
        {
            _viewportX = Math.Max(0, x);
            _viewportY = Math.Max(0, y);
            _viewportWidth = Math.Max(0, width);
            _viewportHeight = Math.Max(0, height);
            _viewportActive = true;
        }

        /// <summary>
        /// Resets the viewport
        /// </summary>
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

        /// <summary>
        /// Disposes the render engine
        /// </summary>
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
}

/// <summary>
/// Border characters for different border styles
/// </summary>
internal struct BorderChars
{
    /// <summary>
    /// Top left corner
    /// </summary>
    public char TopLeft { get; }

    /// <summary>
    /// Top right corner
    /// </summary>
    public char TopRight { get; }

    /// <summary>
    /// Bottom left corner
    /// </summary>
    public char BottomLeft { get; }

    /// <summary>
    /// Bottom right corner
    /// </summary>
    public char BottomRight { get; }

    /// <summary>
    /// Horizontal line
    /// </summary>
    public char Horizontal { get; }

    /// <summary>
    /// Vertical line
    /// </summary>
    public char Vertical { get; }

    /// <summary>
    /// Initializes border characters
    /// </summary>
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