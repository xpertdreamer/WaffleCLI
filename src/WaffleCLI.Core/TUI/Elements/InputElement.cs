using System.Text;
using WaffleCLI.Abstractions.TUI;
using WaffleCLI.Core.TUI.Reactive;

namespace WaffleCLI.Core.TUI.Elements;

public class InputElement : ITuiElement
{
    private readonly ReactiveProperty<string> _value;
    private readonly StringBuilder _inputBuffer;
    private int _cursorPosition = 0;

    public InputElement()
    {
        _value = new ReactiveProperty<string>(string.Empty);
        _value.Subscribe(new ValueObserver(this));
    }

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ComponentState State { get; private set; } = ComponentState.Created;

    public string Value => _value.Value;
    public string Placeholder {get; set;} = "Enter text here...";
    public int MaxLen { get; set; } = 50;

    public event Action<string>? ValueChanged;
    public event Action? RequestRedraw;
    
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 1;
    public bool isVisible { get; set; } = true;
    public bool isFocusable { get; set; } = true;
    public bool HasFocus { get; set; }
    
    public async Task OnCreateAsync()
    {
        State = ComponentState.Created;
        await Task.CompletedTask;
    }
    
    public async Task OnRenderAsync()
    {
        State = ComponentState.Rendering;
        Render();
        await Task.CompletedTask;
    }

    public void Render()
    {
        if (!isVisible) return;

        var displayText = GetDisplayText();

        Console.BackgroundColor = HasFocus ? ConsoleColor.DarkBlue : ConsoleColor.Black;
        Console.ForegroundColor = HasFocus ? ConsoleColor.White :
            string.IsNullOrEmpty(_value.Value) ? ConsoleColor.DarkGray : ConsoleColor.White;
        
        Console.SetCursorPosition(X, Y);
        Console.Write(displayText.PadRight(Width));

        if (HasFocus)
        {
            Console.SetCursorPosition(X + _cursorPosition, Y);
        }
        
        Console.ResetColor();
    }
    
    private string GetDisplayText()
    {
        if (string.IsNullOrEmpty(_value.Value) && !HasFocus)
            return Placeholder;

        return _value.Value;
    }

    public bool HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (!HasFocus) return false;

        switch (keyInfo.Key)
        {
            case ConsoleKey.Backspace:
                if (_cursorPosition > 0 && _inputBuffer.Length > 0)
                {
                    _inputBuffer.Remove(_inputBuffer.Length - 1, 1);
                    _cursorPosition--;
                    UpdateValue();
                }
                return true;
            
            case ConsoleKey.Delete:
                if (_cursorPosition < _inputBuffer.Length)
                {
                    _inputBuffer.Remove(_cursorPosition, 1);
                    UpdateValue();
                }
                return true;
            
            case ConsoleKey.LeftArrow:
                if (_cursorPosition > 0)
                    _cursorPosition--;
                return true;

            case ConsoleKey.RightArrow:
                if (_cursorPosition < _inputBuffer.Length)
                    _cursorPosition++;
                return true;

            case ConsoleKey.Home:
                _cursorPosition = 0;
                return true;

            case ConsoleKey.End:
                _cursorPosition = _inputBuffer.Length;
                return true;
            
            default:
                if (keyInfo.KeyChar >= 32 && keyInfo.KeyChar <= 126 && _inputBuffer.Length < MaxLen)
                {
                    _inputBuffer.Insert(_cursorPosition, keyInfo.KeyChar);
                    _cursorPosition++;
                    UpdateValue();
                    return true;
                }
                break;
        }
        
        return false;
    }
    
    private void UpdateValue()
    {
        _value.Value = _inputBuffer.ToString();
    }

    public Task OnDestroyAsync()
    {
        State = ComponentState.Destroyed;
        return Task.CompletedTask;
    }
    
    public Task OnResizeAsync(int width, int height)
    {
        RequestRedraw?.Invoke();
        return Task.CompletedTask;
    }

    private class ValueObserver : IObserver<string>
    {
        private readonly InputElement _parent;

        public ValueObserver(InputElement parent)
        {
            _parent = parent;
        }

        public void OnNext(string value)
        {
            _parent.ValueChanged?.Invoke(value);
            _parent.RequestRedraw?.Invoke();
        }

        public void OnError(Exception error) {}
        public void OnCompleted() {}
    }
}