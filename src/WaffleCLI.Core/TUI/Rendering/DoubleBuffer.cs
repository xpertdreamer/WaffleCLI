using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// Highly optimized double buffer with minimal console operations
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
            // Only clear if colors changed or buffer is dirty
            bool needsClear = _dirtyLineCount > 0;
            
            for (int i = 0; i < _backBuffer.Length; i++)
            {
                _backBuffer[i] = ' ';
                _backForeground[i] = colors.Foreground;
                _backBackground[i] = colors.Background;
            }
            
            // Mark all lines as dirty
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
            Console.SetCursorPosition(0, 0);
            
            for (int y = 0; y < _height; y++)
            {
                RenderLine(y);
                
                // Move to next line if not last
                if (y < _height - 1)
                {
                    Console.WriteLine();
                }
            }
        }

        private void RenderDirtyLines()
        {
            for (int y = 0; y < _height; y++)
            {
                if (_dirtyLines[y])
                {
                    Console.SetCursorPosition(0, y);
                    RenderLine(y);
                }
            }
        }

        private void RenderLine(int y)
        {
            int lineStart = y * _width;
            ConsoleColor currentFg = _frontForeground[lineStart];
            ConsoleColor currentBg = _frontBackground[lineStart];
            
            Console.ForegroundColor = currentFg;
            Console.BackgroundColor = currentBg;

            // Build line efficiently
            var lineBuilder = new System.Text.StringBuilder(_width);
            
            for (int x = 0; x < _width; x++)
            {
                int index = lineStart + x;
                var cellFg = _frontForeground[index];
                var cellBg = _frontBackground[index];
                
                // Only change colors when necessary
                if (cellFg != currentFg || cellBg != currentBg)
                {
                    if (lineBuilder.Length > 0)
                    {
                        Console.Write(lineBuilder.ToString());
                        lineBuilder.Clear();
                    }
                    Console.ForegroundColor = cellFg;
                    Console.BackgroundColor = cellBg;
                    currentFg = cellFg;
                    currentBg = cellBg;
                }
                
                lineBuilder.Append(_frontBuffer[index]);
            }
            
            // Write remaining characters
            if (lineBuilder.Length > 0)
            {
                Console.Write(lineBuilder.ToString());
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