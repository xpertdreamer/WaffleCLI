using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Rendering
{
    /// <summary>
    /// Double buffer implementation for flicker-free rendering
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
            
            // TuiLogger.LogInfo($"DoubleBuffer created: {width}x{height} (total cells: {bufferSize})");
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
            TuiLogger.LogDebug($"Buffer cleared with colors: {colors.Foreground}/{colors.Background}");
        }

        public void Swap()
        {
            Array.Copy(_backBuffer, _frontBuffer, _backBuffer.Length);
            Array.Copy(_backForeground, _frontForeground, _backForeground.Length);
            Array.Copy(_backBackground, _frontBackground, _backBackground.Length);
        }

        public void RenderToConsole()
        {
            try
            {
                int changedPixels = 0;
                Console.SetCursorPosition(0, 0);
                
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        int index = y * _width + x;
                        
                        // Only update changed pixels
                        if (_frontBuffer[index] != _backBuffer[index] ||
                            _frontForeground[index] != _backForeground[index] ||
                            _frontBackground[index] != _backBackground[index])
                        {
                            changedPixels++;
                            Console.SetCursorPosition(x, y);
                            Console.ForegroundColor = _backForeground[index];
                            Console.BackgroundColor = _backBackground[index];
                            Console.Write(_backBuffer[index]);
                        }
                    }
                }
                
                if (changedPixels > 0)
                {
                    TuiLogger.LogDebug($"Rendered {changedPixels} changed pixels");
                }
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Error in RenderToConsole", ex);
            }
            finally
            {
                Console.ResetColor();
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
                TuiLogger.LogInfo("DoubleBuffer disposed");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}