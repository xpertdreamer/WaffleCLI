using System.Diagnostics;
using System.Text;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Diagnostics;

namespace WaffleCLI.Core.TUI.Rendering;

public class DoubleBufferedRenderEngine : IRenderEngine, IDisposable
{
    #region Nested Types
    private class Cell : IEquatable<Cell>
    {
        public char Character { get; set; } = ' ';
        public ConsoleColor Foreground {get; set;} = ConsoleColor.White;
        public ConsoleColor Background {get; set;} = ConsoleColor.Black;
        public bool IsDirty { get; set; } = true;
        
        
        public bool Equals(Cell? other)
        {
            if (other is null) return false;
            return Character == other.Character && 
                   Foreground == other.Foreground && 
                   Background == other.Background;
        }
        
        public override bool Equals(object? obj) => Equals(obj as Cell);
        public override int GetHashCode() => HashCode.Combine(Character, Foreground, Background);
    }
    
    private record struct DirtyRegion(int X, int Y, int Width, int Height)
    {
        public static DirtyRegion FromRectangle(Rectangle rect) => 
            new(rect.X, rect.Y, rect.Width, rect.Height);
    }
    #endregion
    
    private Cell[,] _frontBuffer;
    private Cell[,] _backBuffer;
    private readonly List<DirtyRegion> _dirtyRegions = [];
    private readonly bool _enablePartialRendering;
    private readonly Stopwatch _renderStopwatch = new();
    private readonly object _renderLock = new object();
    private bool _isInitialized = false;
    private bool _disposed = false;
    private int _totalFrames = 0;
    private int _totalFlushes = 0;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool SupportsPartialRendering => _enablePartialRendering;
    public RenderStats LastRenderStats { get; private set; }
    public int TotalFrames => _totalFrames;
    public int TotalFlushes => _totalFlushes;

    public DoubleBufferedRenderEngine(bool enablePartialRendering = true)
    {
        _enablePartialRendering = enablePartialRendering;
        InitializeSafeDimensions();
        InitializeBuffers();
        _isInitialized = true;
        
        TuiDiagnosticsService.Instance.Log("DebugDoubleBufferedRenderEngine initialized");
    }
    
    private void InitializeSafeDimensions()
    {
        try
        {
            Width = Math.Max(1, Console.WindowWidth);
            Height = Math.Max(1, Console.WindowHeight);
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"Failed to get console dimensions: {ex.Message}");
            Width = 80;
            Height = 25;
        }
    }

    private void InitializeBuffers()
    {
        _frontBuffer = new Cell[Width, Height];
        _backBuffer = new Cell[Width, Height];
        
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _frontBuffer[x, y] = new Cell();
                _backBuffer[x, y] = new Cell();
            }
        }
    }

    public void Initialize(int width, int height)
    {
        lock (_renderLock)
        {
            if (_disposed) return;
            
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            
            InitializeBuffers();
            _isInitialized = true;
            
            TuiDiagnosticsService.Instance.Log($"RenderEngine initialized: {Width}x{Height}");
        }
    }

    public void BeginFrame()
    {
        if (!_isInitialized || _disposed) return;

        lock (_renderLock)
        {
            _renderStopwatch.Restart();
            _dirtyRegions.Clear();

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _backBuffer[x, y].IsDirty = false;
                }
            }
        }
    }

    public void EndFrame()
    {
        if (!_isInitialized || _disposed) return;

        lock (_renderLock)
        {
            if (_enablePartialRendering)
                CalculateDirtyRegions();

            LastRenderStats = new RenderStats(
                ElementRendered: 0,
                CharactersDrawn: CountDirtyCells(),
                DirtyRegion: _dirtyRegions.Count,
                RenderTimeMs: _renderStopwatch.Elapsed.TotalMilliseconds,
                FlushTimeMs: 0
            );
            
            if (_totalFrames % 100 == 0)
            {
                TuiDiagnosticsService.Instance.Log($"Frame {_totalFrames}: {LastRenderStats}");
            }
        }
    }

    public void RenderElement(ITuiElement element)
    {
        if(!_isInitialized ||!element.isVisible || _disposed) return;
        
        TuiDiagnosticsService.Instance.Log($"Rendering element: {element.GetType().Name} at ({element.X}, {element.Y})");
        element.Render();
    }

    public void RenderText(int x, int y, string text, ConsoleColor fg, ConsoleColor bg)
    {
        if (!_isInitialized || string.IsNullOrEmpty(text) || _disposed) return;

        lock (_renderLock)
        {
            TuiDiagnosticsService.Instance.Log($"Rendering text: '{text}' at ({x}, {y})");
            
            for (var i = 0; i < text.Length; i++)
            {
                var charX = x + i;
                if (charX >= Width || charX < 0 || y >= Height || y < 0)
                    continue;

                SetCell(charX, y, text[i], fg, bg);
            }
        }
    }

    public void RenderRect(int x, int y, int width, int height, ConsoleColor color, char fillChar = ' ')
    {
        if (!_isInitialized || _disposed) return;
        
        lock (_renderLock)
        {
            TuiDiagnosticsService.Instance.Log($"Rendering rect: ({x}, {y}) {width}x{height}");
            
            for (var rectY = y; rectY < y + height; rectY++)
            {
                for (var rectX = x; rectX < x + width; rectX++)
                {
                    if (rectX < 0 || rectX >= Width || rectY < 0 || rectY >= Height)
                        continue;

                    SetCell(rectX, rectY, fillChar, color, color);
                }
            }
        }
    }

    public void RenderBorder(int x, int y, int width, int height, BorderStyle borderStyle)
    {
        if (!_isInitialized || _disposed) return;
        
        TuiDiagnosticsService.Instance.Log($"Rendering border: ({x}, {y}) {width}x{height}");
        
        var (horizontal, vertical, topLeft, topRight, bottomRight, bottomLeft) = GetBorderChars(borderStyle);

        lock (_renderLock)
        {
            SetCellSafe(x, y, topLeft, ConsoleColor.White, ConsoleColor.Black);
            SetCellSafe(x + width - 1, y, topRight, ConsoleColor.White, ConsoleColor.Black);
            SetCellSafe(x, y + height - 1, bottomLeft, ConsoleColor.White, ConsoleColor.Black);
            SetCellSafe(x + width - 1, y + height - 1, bottomRight, ConsoleColor.White, ConsoleColor.Black);

            for (var i = 1; i < width - 1; i++)
            {
                SetCellSafe(x + i, y, horizontal, ConsoleColor.White, ConsoleColor.Black);
                SetCellSafe(x + i, y + height - 1, horizontal, ConsoleColor.White, ConsoleColor.Black);
            }

            for (var i = 1; i < height - 1; i++)
            {
                SetCellSafe(x, y + i, vertical, ConsoleColor.White, ConsoleColor.Black);
                SetCellSafe(x + width - 1, y + i, vertical, ConsoleColor.White, ConsoleColor.Black);
            }
        }
    }
    
    private void SetCellSafe(int x, int y, char character, ConsoleColor foreground, ConsoleColor background)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            SetCell(x, y, character, foreground, background);
        }
    }

    private static (char horizontal, char vertical, char topLeft, char topRight, char bottomRight, char bottomLeft)
        GetBorderChars(BorderStyle borderStyle)
    {
        return borderStyle switch
        {
            // So, will implement it later
            BorderStyle.Single => ('-', '|', '+', '+', '+', '+'),
            BorderStyle.Double => ('═', '|', '+', '+', '+', '+'),
            BorderStyle.Rounded => ('-', '|', '+', '+', '+', '+'),
            BorderStyle.Thick => ('-', '|', '+', '+', '+', '+'),
            BorderStyle.Dashed => ('-', '|', '+', '+', '+', '+'),
            _ => ('─', '│', '┌', '┐', '└', '┘')
        };
    }

    private void SetCell(int x, int y, char character, ConsoleColor foreground, ConsoleColor background)
    {
        if (!_isInitialized ||x < 0 || x >= Width || y < 0 || y >= Height) return;
        
        var cell = _backBuffer[x, y];
        cell.Character = character;
        cell.Foreground =  foreground;
        cell.Background = background;
        cell.IsDirty = true;
    }

    public void Clear()
    {
        if (!_isInitialized || _disposed) return;

        lock (_renderLock)
        {
            TuiDiagnosticsService.Instance.Log("Clearing render buffer");
            
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    SetCell(x, y, ' ', ConsoleColor.White, ConsoleColor.Black);
                }
            }
        }
    }
    
    public void ClearArea(int x, int y, int width, int height)
    {
        if (!_isInitialized || _disposed) return;
        
        lock (_renderLock)
        {
            TuiDiagnosticsService.Instance.Log($"Clearing area: ({x}, {y}) {width}x{height}");
            
            for (var rectY = y; rectY < y + height; rectY++)
            {
                for (var rectX = x; rectX < x + width; rectX++)
                {
                    if (rectX >= 0 && rectX < Width && rectY >= 0 && rectY < Height)
                    {
                        SetCell(rectX, rectY, ' ', ConsoleColor.White, ConsoleColor.Black);
                    }
                }
            }
        }
    }
    
    public void SetCursorPosition(int x, int y)
    {
        if (!_isInitialized) return;
        
        try
        {
            if (x >= 0 && x < Console.WindowWidth && y >= 0 && y < Console.WindowHeight)
            {
                Console.SetCursorPosition(x, y);
            }
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"SetCursorPosition error: {ex.Message}");
        }
    }

    public void Flush()
    {
        if (!_isInitialized || _disposed) return;

        var flushStopwatch = Stopwatch.StartNew();
        
        try
        {
            lock (_renderLock)
            {
                if (_enablePartialRendering && _dirtyRegions.Count > 0)
                    FlushPartial();
                else
                    FlushFull();

                LastRenderStats = LastRenderStats with { FlushTimeMs = flushStopwatch.Elapsed.TotalMilliseconds };
            }
        }
        catch (Exception ex)
        {
            TuiDiagnosticsService.Instance.Log($"Flush error: {ex}");
        }
    }

    private void FlushPartial()
    {
        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;
            
        try
        {
            TuiDiagnosticsService.Instance.Log($"Flushing {_dirtyRegions.Count} dirty regions");
            foreach (var region in _dirtyRegions)
            {
                FlushRegion(region);
            }

            SyncBuffers();
        }
        finally
        {
            Console.ForegroundColor = currentFg;
            Console.BackgroundColor = currentBg;
        }
    }

    private void FlushFull()
    {
        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;

        try
        {
            TuiDiagnosticsService.Instance.Log("Flushing full buffer");
            
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var backCell = _backBuffer[x, y];
                    var frontCell = _frontBuffer[x, y];

                    if (!backCell.IsDirty || backCell.Equals(frontCell)) 
                        continue;

                    // Only write if position is valid
                    if (x < Console.WindowWidth && y < Console.WindowHeight)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = backCell.Foreground;
                        Console.BackgroundColor = backCell.Background;
                        Console.Write(backCell.Character);
                    }

                    frontCell.Character = backCell.Character;
                    frontCell.Foreground = backCell.Foreground;
                    frontCell.Background = backCell.Background;
                }
            }
        }
        finally
        {
            Console.ForegroundColor = currentFg;
            Console.BackgroundColor = currentBg;
        }
    }

    private void FlushRegion(DirtyRegion region)
    {
        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;

        try
        {
            for (var y = region.Y; y < region.Y + region.Height; y++)
            {
                for (var x = region.X; x < region.X + region.Width; x++)
                {
                    if (x >= Width || y >= Height) continue;

                    var backCell = _backBuffer[x, y];
                    var frontCell = _frontBuffer[x, y];

                    if (!backCell.IsDirty || backCell.Equals(frontCell)) 
                        continue;

                    // Only write if position is valid
                    if (x < Console.WindowWidth && y < Console.WindowHeight)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = backCell.Foreground;
                        Console.BackgroundColor = backCell.Background;
                        Console.Write(backCell.Character);
                    }

                    frontCell.Character = backCell.Character;
                    frontCell.Foreground = backCell.Foreground;
                    frontCell.Background = backCell.Background;
                }
            }
        }
        finally
        {
            Console.ForegroundColor = currentFg;
            Console.BackgroundColor = currentBg;
        }
    }
    
    private void SyncBuffers()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (!_backBuffer[x, y].IsDirty) continue;
                
                _frontBuffer[x, y].Character = _backBuffer[x, y].Character;
                _frontBuffer[x, y].Foreground = _backBuffer[x, y].Foreground;
                _frontBuffer[x, y].Background = _backBuffer[x, y].Background;
            }
        }
    }

    private void CalculateDirtyRegions()
    {
        _dirtyRegions.Clear();
        var visited = new bool[Width, Height];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (!_backBuffer[x, y].IsDirty || visited[x, y]) continue;
                
                var region = GrowRegion(x, y, visited);
                _dirtyRegions.Add(DirtyRegion.FromRectangle(region));
            }
        }
    }

    private int CountDirtyCells()
    {
        var count = 0;
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (_backBuffer[x, y].IsDirty)
                    count++;
            }
        }

        return count;
    }

    private Rectangle GrowRegion(int startX, int startY, bool[,] visited)
    {
        var stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        int minX = startX, maxX = startX;
        int minY = startY, maxY = startY;

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            
            if (x < 0 || x >= Width || y < 0 || y >= Height || visited[x, y] || !_backBuffer[x, y].IsDirty)
                continue;
            
            visited[x, y] = true;
            
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
            
            if (x > 0) stack.Push((x - 1, y));
            if (x < Width - 1) stack.Push((x + 1, y));
            if (y > 0) stack.Push((x, y - 1));
            if (y < Height - 1) stack.Push((x, y + 1));
        }
        
        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
    
    public void Dispose()
    {
        if (_disposed) return;

        lock (_renderLock)
        {
            _disposed = true;
            _frontBuffer = null!;
            _backBuffer = null!;
            _dirtyRegions.Clear();
            
            TuiDiagnosticsService.Instance.Log("DebugDoubleBufferedRenderEngine disposed");
        }
        
        GC.SuppressFinalize(this);
    }
}

public record Rectangle(int X, int Y, int Width, int Height);