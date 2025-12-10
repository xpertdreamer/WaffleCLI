namespace WaffleCLI.Abstractions.TUI.Input
{
    /// <summary>
    /// Represents an input event
    /// </summary>
    public struct InputEvent
    {
        public ConsoleKey Key { get; set; }
        public char Character { get; set; }
        public KeyModifiers Modifiers { get; set; }
        public DateTime Timestamp { get; set; }
        
        public override string ToString()
        {
            return $"Key: {Key}, Char: {Character}, Modifiers: {Modifiers}";
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Control = 1,
        Alt = 2,
        Shift = 4
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    /// <summary>
    /// Input event extensions for easier handling
    /// </summary>
    public static class InputEventExtensions
    {
        /// <summary>
        /// Checks if the input event matches a specific key combination
        /// </summary>
        public static bool Is(this InputEvent input, ConsoleKey key, KeyModifiers modifiers = KeyModifiers.None)
        {
            return input.Key == key && input.Modifiers == modifiers;
        }
        
        /// <summary>
        /// Checks if the input event matches any of the specified key combinations
        /// </summary>
        public static bool IsAny(this InputEvent input, params (ConsoleKey key, KeyModifiers modifiers)[] combinations)
        {
            foreach (var (key, modifiers) in combinations)
            {
                if (input.Key == key && input.Modifiers == modifiers)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Checks if the input is a printable character
        /// </summary>
        public static bool IsPrintable(this InputEvent input)
        {
            return !char.IsControl(input.Character) && input.Character >= 32;
        }
        
        /// <summary>
        /// Checks if the input is a navigation key
        /// </summary>
        public static bool IsNavigation(this InputEvent input)
        {
            return input.Key == ConsoleKey.UpArrow || 
                   input.Key == ConsoleKey.DownArrow || 
                   input.Key == ConsoleKey.LeftArrow || 
                   input.Key == ConsoleKey.RightArrow ||
                   input.Key == ConsoleKey.Tab ||
                   input.Key == ConsoleKey.PageUp ||
                   input.Key == ConsoleKey.PageDown ||
                   input.Key == ConsoleKey.Home ||
                   input.Key == ConsoleKey.End;
        }
        
        /// <summary>
        /// Checks if the input is an action key (Enter, Space, Escape)
        /// </summary>
        public static bool IsAction(this InputEvent input)
        {
            return input.Key == ConsoleKey.Enter || 
                   input.Key == ConsoleKey.Spacebar || 
                   input.Key == ConsoleKey.Escape;
        }
    }
}