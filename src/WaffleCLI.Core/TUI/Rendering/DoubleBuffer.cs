using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// Optimized double buffer with precise cursor control to prevent scrolling
    /// </summary>
    public class DoubleBuffer : IBuffer
    {
        private readonly char[] _frontBuffer;
        private readonly char[] _backBuffer;
        private readonly ConsoleColor[] _frontForeground;
        private readonly ConsoleColor[] _backForeground;
        private readonly ConsoleColor[] _frontBackground;
        private readonly ConsoleColor[] _backBackground;
        private readonly int _width;
        private readonly int _height;
        private bool _disposed = false;
        private bool _firstRender = true;
        private readonly bool[] _dirtyLines;
        private int _dirtyLineCount = 0;
        private ConsoleColor _clearForeground = ConsoleColor.Gray;
        private ConsoleColor _clearBackground = ConsoleColor.Black;

        public int Width => _width;
        public int Height => _height;

        public DoubleBuffer(int width, int height)
        {
            _width = width;
            _height = height;
            
            int bufferSize = width * height;
            _frontBuffer = new char[bufferSize];
            _backBuffer = new char[bufferSize];
            _frontForeground = new ConsoleColor[bufferSize];
            _backForeground = new ConsoleColor[bufferSize];
            _frontBackground = new ConsoleColor[bufferSize];
            _backBackground = new ConsoleColor[bufferSize];
            _dirtyLines = new bool[height];
            
            ClearBuffers();
        }

        public void SetPixel(int x, int y, char character, ConsoleColor foreground, ConsoleColor background)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return;

            int index = y * _width + x;
            
            // Only mark dirty if actually changed
            if (_backBuffer[index] != character || 
                _backForeground[index] != foreground || 
                _backBackground[index] != background)
            {
                _backBuffer[index] = character;
                _backForeground[index] = foreground;
                _backBackground[index] = background;
                
                if (!_dirtyLines[y])
                {
                    _dirtyLines[y] = true;
                    _dirtyLineCount++;
                }
            }
        }

        public void Clear(ColorScheme colors)
        {
            _clearForeground = colors.Foreground;
            _clearBackground = colors.Background;

            // Clear back buffer with specified colors
            for (int i = 0; i < _backBuffer.Length; i++)
            {
                _backBuffer[i] = ' ';
                _backForeground[i] = colors.Foreground;
                _backBackground[i] = colors.Background;
            }
            
            // Mark all lines as dirty for next render
            for (int y = 0; y < _height; y++)
            {
                if (!_dirtyLines[y])
                {
                    _dirtyLines[y] = true;
                    _dirtyLineCount++;
                }
            }
        }

        public void Swap()
        {
            // Swap buffers
            Array.Copy(_backBuffer, _frontBuffer, _backBuffer.Length);
            Array.Copy(_backForeground, _frontForeground, _backForeground.Length);
            Array.Copy(_backBackground, _frontBackground, _backBackground.Length);
        }

        public void RenderToConsole()
        {
            try
            {
                if (_firstRender || _dirtyLineCount > _height / 2)
                {
                    // Full render for first frame or many changes
                    RenderFullFrame();
                    _firstRender = false;
                }
                else if (_dirtyLineCount > 0)
                {
                    // Optimized render for few changes
                    RenderDirtyLines();
                }
                
                // Reset dirty tracking
                Array.Fill(_dirtyLines, false);
                _dirtyLineCount = 0;
            }
            catch (Exception ex)
            {
                // Fallback to full render on error
                RenderFullFrame();
                Array.Fill(_dirtyLines, false);
                _dirtyLineCount = 0;
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private void RenderFullFrame()
        {
            // For full frame render, we render all lines
            for (int y = 0; y < _height; y++)
            {
                RenderLine(y);
            }
            
            // Ensure cursor is at the bottom after rendering
            Console.SetCursorPosition(0, Math.Min(_height, Console.WindowHeight - 1));
        }

        private void RenderDirtyLines()
        {
            for (int y = 0; y < _height; y++)
            {
                if (_dirtyLines[y])
                {
                    RenderLine(y);
                }
            }
            
            // Ensure cursor is at the bottom after rendering
            Console.SetCursorPosition(0, Math.Min(_height, Console.WindowHeight - 1));
        }

        private void RenderLine(int y)
        {
            int lineStart = y * _width;
            
            // Position cursor at the beginning of the line
            // Ensure we don't try to write beyond console bounds
            if (y >= Console.WindowHeight) return;
            
            Console.SetCursorPosition(0, y);
            
            ConsoleColor currentFg = _frontForeground[lineStart];
            ConsoleColor currentBg = _frontBackground[lineStart];
            
            // Build and render the entire line at once with proper color spans
            var spans = new List<(string text, ConsoleColor fg, ConsoleColor bg)>();
            string currentSpan = "";
            ConsoleColor spanFg = currentFg;
            ConsoleColor spanBg = currentBg;
            
            for (int x = 0; x < _width; x++)
            {
                int index = lineStart + x;
                var cellFg = _frontForeground[index];
                var cellBg = _frontBackground[index];
                
                // Check if we need to start a new color span
                if (cellFg != spanFg || cellBg != spanBg)
                {
                    if (!string.IsNullOrEmpty(currentSpan))
                    {
                        spans.Add((currentSpan, spanFg, spanBg));
                    }
                    
                    spanFg = cellFg;
                    spanBg = cellBg;
                    currentSpan = _frontBuffer[index].ToString();
                }
                else
                {
                    currentSpan += _frontBuffer[index];
                }
            }
            
            // Add the last span
            if (!string.IsNullOrEmpty(currentSpan))
            {
                spans.Add((currentSpan, spanFg, spanBg));
            }
            
            // Render all spans
            foreach (var span in spans)
            {
                Console.ForegroundColor = span.fg;
                Console.BackgroundColor = span.bg;
                Console.Write(span.text);
            }
            
            // Clear the rest of the line if console is wider than buffer
            int remainingWidth = Console.WindowWidth - _width;
            if (remainingWidth > 0)
            {
                Console.Write(new string(' ', remainingWidth));
            }
        }

        private void ClearBuffers()
        {
            for (int i = 0; i < _backBuffer.Length; i++)
            {
                _frontBuffer[i] = ' ';
                _backBuffer[i] = ' ';
                _frontForeground[i] = ConsoleColor.Gray;
                _backForeground[i] = ConsoleColor.Gray;
                _frontBackground[i] = ConsoleColor.Black;
                _backBackground[i] = ConsoleColor.Black;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}