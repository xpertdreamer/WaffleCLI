using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// High-performance double buffer with smart rendering
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
        private int _changeCount = 0;
        private const int DIFFERENTIAL_THRESHOLD = 500; // Use differential if fewer than 500 changes

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
            
            ClearBuffers();
        }

        public void SetPixel(int x, int y, char character, ConsoleColor foreground, ConsoleColor background)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                int index = y * _width + x;
                _backBuffer[index] = character;
                _backForeground[index] = foreground;
                _backBackground[index] = background;
                _changeCount++;
            }
        }

        public void Clear(ColorScheme colors)
        {
            for (int i = 0; i < _backBuffer.Length; i++)
            {
                _backBuffer[i] = ' ';
                _backForeground[i] = colors.Foreground;
                _backBackground[i] = colors.Background;
            }
            _changeCount = int.MaxValue; // Force full render on clear
        }

        public void Swap()
        {
            Array.Copy(_backBuffer, _frontBuffer, _backBuffer.Length);
            Array.Copy(_backForeground, _frontForeground, _backForeground.Length);
            Array.Copy(_backBackground, _frontBackground, _backBackground.Length);
        }

        public void RenderToConsole(bool forceFullRender = false)
        {
            try
            {
                // Smart rendering: use differential for small changes, full for large changes
                bool useDifferential = !forceFullRender && _changeCount > 0 && _changeCount < DIFFERENTIAL_THRESHOLD;
                
                if (useDifferential)
                {
                    RenderDifferential();
                }
                else
                {
                    RenderFullFrame();
                }
                
                _changeCount = 0; // Reset change counter
            }
            catch (Exception ex)
            {
                // Fallback to simple rendering on error
                try
                {
                    Console.Clear();
                    RenderFullFrame();
                }
                catch
                {
                    // Last resort
                }
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
                Console.SetCursorPosition(0, y);
                
                for (int x = 0; x < _width; x++)
                {
                    int index = y * _width + x;
                    Console.ForegroundColor = _backForeground[index];
                    Console.BackgroundColor = _backBackground[index];
                    Console.Write(_backBuffer[index]);
                }
                
                // Clear the rest of the line if needed
                if (_width < Console.WindowWidth)
                {
                    Console.Write(new string(' ', Console.WindowWidth - _width));
                }
            }
            
            // Clear any remaining lines
            for (int y = _height; y < Console.WindowHeight; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            
            Console.SetCursorPosition(0, 0);
        }

        private void RenderDifferential()
        {
            int updated = 0;
            ConsoleColor currentFg = ConsoleColor.White;
            ConsoleColor currentBg = ConsoleColor.Black;
            
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = y * _width + x;
                    
                    if (_frontBuffer[index] != _backBuffer[index] ||
                        _frontForeground[index] != _backForeground[index] ||
                        _frontBackground[index] != _backBackground[index])
                    {
                        // Only set cursor position when necessary
                        Console.SetCursorPosition(x, y);
                        
                        // Only change colors when necessary (reduces console API calls)
                        if (currentFg != _backForeground[index] || currentBg != _backBackground[index])
                        {
                            Console.ForegroundColor = _backForeground[index];
                            Console.BackgroundColor = _backBackground[index];
                            currentFg = _backForeground[index];
                            currentBg = _backBackground[index];
                        }
                        
                        Console.Write(_backBuffer[index]);
                        updated++;
                    }
                }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}