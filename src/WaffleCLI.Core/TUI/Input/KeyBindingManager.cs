using WaffleCLI.Abstractions.TUI.Input;

namespace WaffleCLI.Core.TUI.Input
{
    /// <summary>
    /// Manages global hotkeys and key bindings
    /// </summary>
    public class KeyBindingManager
    {
        private readonly Dictionary<Hotkey, Action> _globalHotkeys = new();

        public void RegisterGlobalHotkey(ConsoleKey key, KeyModifiers modifiers, Action action)
        {
            var hotkey = new Hotkey { Key = key, Modifiers = modifiers };
            _globalHotkeys[hotkey] = action;
        }

        public void RegisterGlobalHotkey(InputEvent inputEvent, Action action)
        {
            var hotkey = new Hotkey { Key = inputEvent.Key, Modifiers = inputEvent.Modifiers };
            _globalHotkeys[hotkey] = action;
        }

        public bool TryHandleGlobalHotkey(InputEvent inputEvent)
        {
            var hotkey = new Hotkey { Key = inputEvent.Key, Modifiers = inputEvent.Modifiers };
            
            if (_globalHotkeys.TryGetValue(hotkey, out var action))
            {
                action();
                return true;
            }

            return false;
        }

        public void ClearHotkeys()
        {
            _globalHotkeys.Clear();
        }

        public IReadOnlyDictionary<Hotkey, Action> GetHotkeys()
        {
            return _globalHotkeys;
        }
    }

    public struct Hotkey
    {
        public ConsoleKey Key { get; set; }
        public KeyModifiers Modifiers { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Hotkey other && Equals(other);
        }

        public bool Equals(Hotkey other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Modifiers);
        }

        public override string ToString()
        {
            var modifiers = new List<string>();
            if (Modifiers.HasFlag(KeyModifiers.Control)) modifiers.Add("Ctrl");
            if (Modifiers.HasFlag(KeyModifiers.Alt)) modifiers.Add("Alt");
            if (Modifiers.HasFlag(KeyModifiers.Shift)) modifiers.Add("Shift");
            
            return $"{(modifiers.Any() ? string.Join("+", modifiers) + "+" : "")}{Key}";
        }
    }
}