using System.Diagnostics;
using System.Text;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Diagnostics;

namespace WaffleCLI.Core.TUI.Rendering;

public class DoubleBufferedRenderEngine : IRenderEngine, IDisposable
{
    #region Optimized Data Structures
    private struct Cell : IEquatable<Cell>
    {
        public char Character;
        public ConsoleColor Foreground;
        public ConsoleColor Background;
        public bool IsDirty;

        public readonly bool Equals(Cell other) => 
            Character == other.Character && 
            Foreground == other.Foreground && 
            Background == other.Background;

        public readonly override bool Equals(object? obj) => obj is Cell cell && Equals(cell);
        public readonly override int GetHashCode() => HashCode.Combine(Character, Foreground, Background);
    }

    private readonly record struct DirtyRegion(int X, int Y, int Width, int Height)
    {
        public static DirtyRegion FromRectangle(Rectangle rect) => new(rect.X, rect.Y, rect.Width, rect.Height);
    }
    #endregion

    // Border characters cache for performance
    private static readonly Dictionary<BorderStyle, (char h, char v, char tl, char tr, char bl, char br)> 
        BorderCharsCache = new()
    {
        [BorderStyle.Single] = ('─', '│', '┌', '┐', '└', '┘'),
        [BorderStyle.Double] = ('═', '║', '╔', '╗', '╚', '╝'),
        [BorderStyle.Rounded] = ('─', '│', '╭', '╮', '╰', '╯'),
        [BorderStyle.Thick] = ('━', '┃', '┏', '┓', '┗', '┛'),
        [BorderStyle.Dashed] = ('╌', '╎', '┌', '┐', '└', '┘'),
    };

    private Cell[,] _frontBuffer;
    private Cell[,] _backBuffer;
    private readonly List<Rectangle> _dirtyRegions = [];
    private readonly bool _enablePartialRendering;
    private readonly Stopwatch _renderStopwatch = new();
    private readonly object _renderLock = new();
    private bool _isInitialized = false;
    private bool _disposed = false;
    private int _totalFrames = 0;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool SupportsPartialRendering => _enablePartialRendering;
    public RenderStats LastRenderStats { get; private set; }
    public int TotalFrames => _totalFrames;

    public DoubleBufferedRenderEngine(bool enablePartialRendering = true)
    {
        _enablePartialRendering = enablePartialRendering;
        LastRenderStats = new RenderStats(0, 0, 0, 0, 0);
        InitializeSafeDimensions();
        InitializeBuffers();
        _isInitialized = true;
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
        try
        {
            _frontBuffer = new Cell[Width, Height];
            _backBuffer = new Cell[Width, Height];

            // Initialize with empty cells
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _frontBuffer[x, y] = new Cell { Character = ' ', IsDirty = true };
                    _backBuffer[x, y] = new Cell { Character = ' ', IsDirty = true };
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Buffer initialization failed: {ex.Message}", ex);
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
        }
    }

    public void BeginFrame()
    {
        if (!_isInitialized || _disposed) return;

        lock (_renderLock)
        {
            _renderStopwatch.Restart();
            _dirtyRegions.Clear();

            // Reset dirty flags in back buffer
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
            
            _totalFrames++;
        }
    }

    public void RenderElement(ITuiElement element)
    {
        if (!_isInitialized || !element.isVisible || _disposed) return;
        element.Render();
    }

    public void RenderText(int x, int y, string text, ConsoleColor fg, ConsoleColor bg)
    {
        if (!_isInitialized || string.IsNullOrEmpty(text) || _disposed) return;
        if (y < 0 || y >= Height) return;

        lock (_renderLock)
        {
            var length = Math.Min(text.Length, Width - x);
            if (length <= 0) return;

            // Render only visible portion of text
            for (var i = 0; i < length; i++)
            {
                var charX = x + i;
                if (charX < 0) continue;

                SetCell(charX, y, text[i], fg, bg);
            }
        }
    }

    public void RenderRect(int x, int y, int width, int height, ConsoleColor color, char fillChar = ' ')
    {
        if (!_isInitialized || _disposed) return;
        
        lock (_renderLock)
        {
            // Calculate visible area only
            var startX = Math.Max(0, x);
            var startY = Math.Max(0, y);
            var endX = Math.Min(Width, x + width);
            var endY = Math.Min(Height, y + height);

            for (var rectY = startY; rectY < endY; rectY++)
            {
                for (var rectX = startX; rectX < endX; rectX++)
                {
                    SetCell(rectX, rectY, fillChar, color, color);
                }
            }
        }
    }

    public void RenderBorder(int x, int y, int width, int height, BorderStyle borderStyle)
    {
        if (!_isInitialized || _disposed) return;
        if (width < 2 || height < 2) return;

        // Use cached border characters
        if (!BorderCharsCache.TryGetValue(borderStyle, out var chars))
            chars = BorderCharsCache[BorderStyle.Single];

        var (horizontal, vertical, topLeft, topRight, bottomLeft, bottomRight) = chars;

        lock (_renderLock)
        {
            // Corners
            SetCellSafe(x, y, topLeft, ConsoleColor.White, ConsoleColor.Black);
            SetCellSafe(x + width - 1, y, topRight, ConsoleColor.White, ConsoleColor.Black);
            SetCellSafe(x, y + height - 1, bottomLeft, ConsoleColor.White, ConsoleColor.Black);
            SetCellSafe(x + width - 1, y + height - 1, bottomRight, ConsoleColor.White, ConsoleColor.Black);

            // Horizontal lines
            for (var i = 1; i < width - 1; i++)
            {
                SetCellSafe(x + i, y, horizontal, ConsoleColor.White, ConsoleColor.Black);
                SetCellSafe(x + i, y + height - 1, horizontal, ConsoleColor.White, ConsoleColor.Black);
            }

            // Vertical lines
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

    private void SetCell(int x, int y, char character, ConsoleColor foreground, ConsoleColor background)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        
        ref var cell = ref _backBuffer[x, y];
        
        // Only update if actually changed
        if (cell.Character == character && cell.Foreground == foreground && cell.Background == background)
            return;

        cell.Character = character;
        cell.Foreground = foreground;
        cell.Background = background;
        cell.IsDirty = true;
    }

    public void Clear()
    {
        if (!_isInitialized || _disposed) return;

        lock (_renderLock)
        {
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
            var startX = Math.Max(0, x);
            var startY = Math.Max(0, y);
            var endX = Math.Min(Width, x + width);
            var endY = Math.Min(Height, y + height);

            for (var rectY = startY; rectY < endY; rectY++)
            {
                for (var rectX = startX; rectX < endX; rectX++)
                {
                    SetCell(rectX, rectY, ' ', ConsoleColor.White, ConsoleColor.Black);
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
            TuiDiagnosticsService.Instance.Log($"Flush error: {ex.Message}");
        }
    }

    private void FlushPartial()
    {
        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;
            
        try
        {
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
        if (!_isInitialized || _disposed) return;

        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;

        try
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    ref var backCell = ref _backBuffer[x, y];
                    ref var frontCell = ref _frontBuffer[x, y];

                    if (!backCell.IsDirty || backCell.Equals(frontCell))
                        continue;

                    if (x < Console.WindowWidth && y < Console.WindowHeight && x >= 0 && y >= 0)
                    {
                        try
                        {
                            Console.SetCursorPosition(x, y);
                            Console.ForegroundColor = backCell.Foreground;
                            Console.BackgroundColor = backCell.Background;
                            Console.Write(backCell.Character);
                        }
                        catch (Exception ex)
                        {
                            TuiDiagnosticsService.Instance.Log($"Error writing at ({x}, {y}): {ex.Message}");
                        }
                    }

                    frontCell = backCell;
                }
            }
        }
        finally
        {
            Console.ForegroundColor = currentFg;
            Console.BackgroundColor = currentBg;
        }
    }

    private void FlushRegion(Rectangle region)
    {
        if (!_isInitialized || _disposed) return;
        
        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;

        try
        {
            for (var y = region.Y; y < region.Y + region.Height; y++)
            {
                for (var x = region.X; x < region.X + region.Width; x++)
                {
                    if (x >= Width || y >= Height) continue;

                    ref var backCell = ref _backBuffer[x, y];
                    ref var frontCell = ref _frontBuffer[x, y];

                    if (!backCell.IsDirty || backCell.Equals(frontCell)) 
                        continue;

                    if (x < Console.WindowWidth && y < Console.WindowHeight)
                    {
                        try
                        {
                            Console.SetCursorPosition(x, y);
                            Console.ForegroundColor = backCell.Foreground;
                            Console.BackgroundColor = backCell.Background;
                            Console.Write(backCell.Character);
                        }
                        catch (Exception ex)
                        {
                            TuiDiagnosticsService.Instance.Log($"Error writing at ({x}, {y}): {ex.Message}");
                        }
                    }

                    frontCell = backCell;
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
                if (_backBuffer[x, y].IsDirty)
                {
                    _frontBuffer[x, y] = _backBuffer[x, y];
                }
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
                _dirtyRegions.Add(region);
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
            _frontBuffer = null;
            _backBuffer = null;
            _dirtyRegions.Clear();
        }
        
        GC.SuppressFinalize(this);
    }
}

public record Rectangle(int X, int Y, int Width, int Height);