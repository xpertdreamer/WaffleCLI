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
}