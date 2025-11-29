using System.Diagnostics;
using System.Text;
using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Rendering;

public class DoubleBufferedRenderEngine : IRenderEngine
{
    private class Cell
    {
        public char Character { get; set; } = ' ';
        public ConsoleColor Foreground {get; set;} = ConsoleColor.White;
        public ConsoleColor Background {get; set;} = ConsoleColor.Black;
        public bool IsDirty { get; set; } = true;
        
        
        public bool Equals(Cell other)
        {
            return Character == other.Character && Foreground == other.Foreground && Background == other.Background;
        }
    }

    private readonly Cell[,] _frontBuffer;
    private readonly Cell[,] _backBuffer;
    private readonly List<Rectangle> _dirtyRegions = [];
    private readonly bool _enablePartialRendering;
    private readonly Stopwatch _renderStopwatch = new();
    private readonly StringBuilder _outputBuffer = new();
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool SupportsPartialRendering => _enablePartialRendering;
    
    public RenderStats LastRenderStats { get; private set; }

    public DoubleBufferedRenderEngine(bool enablePartialRendering = true)
    {
        _enablePartialRendering = enablePartialRendering;
        Width = Console.WindowWidth;
        Height = Console.WindowHeight;
        
        _frontBuffer = new Cell[Width, Height];
        _backBuffer = new Cell[Width, Height];

        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
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
        Width = width;
        Height = height;
        InitializeBuffers();
    }

    public void BeginFrame()
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

    public void EndFrame()
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
    }

    public void RenderElement(ITuiElement element)
    {
        if(!element.isVisible) return;
        
        element.Render();
    }

    public void RenderText(int x, int y, string text, ConsoleColor fg, ConsoleColor bg)
    {
        if (string.IsNullOrEmpty(text)) return;

        for (var i = 0; i < text.Length; i++)
        {
            var charX = x + i;
            if (charX >= Width || charX < 0 || y >= Height || y < 0)
                continue;
            
            SetCell(charX, y, text[i], fg, bg);
        }
    }

    public void RenderRect(int x, int y, int width, int height, ConsoleColor color, char fillChar = ' ')
    {
        for (var rectY = y; rectY < y + height; rectY++)
        {
            for (var rectX = x; rectX < x + width; rectX++)
            {
                if (rectX >= Width || rectX < 0 || rectY >= Height || rectY < 0)
                    continue;

                SetCell(rectX, rectY, fillChar, color, color);
            }
        }
    }

    public void RenderBorder(int x, int y, int width, int height, BorderStyle borderStyle)
    {
        var (horizontal, vertical, topLeft, topRight, bottomRight, bottomLeft) = GetBorderChars(borderStyle);
        
        SetCell(x, y, topLeft, ConsoleColor.White, ConsoleColor.Black);
        SetCell(x + width - 1, y, topRight, ConsoleColor.White, ConsoleColor.Black);
        SetCell(x, y + height - 1, bottomLeft, ConsoleColor.White, ConsoleColor.Black);
        SetCell(x + width - 1, y + height - 1, bottomRight, ConsoleColor.White, ConsoleColor.Black);
        
        for (var i = 1; i < width - 1; i++)
        {
            SetCell(x + i, y, horizontal, ConsoleColor.White, ConsoleColor.Black);
            SetCell(x + i, y + height - 1, horizontal, ConsoleColor.White, ConsoleColor.Black);
        }
        
        for (var i = 1; i < height - 1; i++)
        {
            SetCell(x, y + i, vertical, ConsoleColor.White, ConsoleColor.Black);
            SetCell(x + width - 1, y + i, vertical, ConsoleColor.White, ConsoleColor.Black);
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
        if (x < 0 || x >= Width || y < 0 || y >= Height) return;
        
        var cell = _backBuffer[x, y];
        cell.Character = character;
        cell.Foreground =  foreground;
        cell.Background = background;
        cell.IsDirty = true;
    }

    public void Clear()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                SetCell(x, y, ' ', ConsoleColor.White, ConsoleColor.Black);
            }
        }
    }
    
    public void ClearArea(int x, int y, int width, int height)
    {
        for (var rectY = y; rectY < y + height; rectY++)
        {
            for (var rectX = x; rectX < x + width; rectX++)
            {
                SetCell(rectX, rectY, ' ', ConsoleColor.White, ConsoleColor.Black);
            }
        }
    }
    
    public void SetCursorPosition(int x, int y)
    {
        // We will process the cursor separately
    }

    public void Flush()
    {
        var flushStopwatch = Stopwatch.StartNew();

        if (_enablePartialRendering && _dirtyRegions.Count > 0)
            FlushPartial();
        else
            FlushFull();
        
        LastRenderStats = LastRenderStats with {FlushTimeMs = flushStopwatch.Elapsed.TotalMilliseconds};
    }

    private void FlushPartial()
    {
        foreach (var region in _dirtyRegions)
        {
            FlushRegion(region);
        }
        
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

    private void FlushFull()
    {
        _outputBuffer.Clear();
        var currentFg = Console.ForegroundColor;
        var currentBg = Console.BackgroundColor;

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var backCell = _backBuffer[x, y];
                var frontCell = _frontBuffer[x, y];

                if (backCell.Equals(frontCell) && !backCell.IsDirty) continue;
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = backCell.Foreground;
                Console.BackgroundColor = backCell.Background;
                Console.Write(backCell.Character);
                    
                frontCell.Character = backCell.Character;
                frontCell.Foreground = backCell.Foreground;
                frontCell.Background = backCell.Background;
            }
        }
        
        Console.ForegroundColor = currentFg;
        Console.BackgroundColor = currentBg;
    }

    private void FlushRegion(Rectangle region)
    {
        var currentFg =  Console.ForegroundColor;
        var currentBg =  Console.BackgroundColor;
        
        for (var y = region.Y; y < region.Y + region.Height; y++)
        {
            for (var x = region.X; x < region.X + region.Width; x++)
            {
                if (x >= Width || y >= Height) continue;

                var backCell = _backBuffer[x, y];
                var frontCell = _frontBuffer[x, y];

                if (!backCell.IsDirty || backCell.Equals(frontCell)) continue;
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = backCell.Foreground;
                Console.BackgroundColor = backCell.Background;
                Console.Write(backCell.Character);

                frontCell.Character = backCell.Character;
                frontCell.Foreground = backCell.Foreground;
                frontCell.Background = backCell.Background;
            }
        }
        
        Console.ForegroundColor = currentFg;
        Console.BackgroundColor = currentBg;
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
            
            stack.Push((x - 1, y));
            stack.Push((x + 1, y));
            stack.Push((x, y - 1));
            stack.Push((x, y + 1));
        }
        
        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}

public record Rectangle(int X, int Y, int Width, int Height);