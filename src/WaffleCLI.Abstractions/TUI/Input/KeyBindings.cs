namespace WaffleCLI.Abstractions.TUI.Input
{
    /// <summary>
    /// Common key bindings
    /// </summary>
    public static class KeyBindings
    {
        public static readonly InputEvent FocusNext = new InputEvent 
        { 
            Key = ConsoleKey.Tab, 
            Modifiers = KeyModifiers.None 
        };
        
        public static readonly InputEvent FocusPrevious = new InputEvent 
        { 
            Key = ConsoleKey.Tab, 
            Modifiers = KeyModifiers.Shift 
        };
        
        public static readonly InputEvent Confirm = new InputEvent 
        { 
            Key = ConsoleKey.Enter, 
            Modifiers = KeyModifiers.None 
        };
        
        public static readonly InputEvent Cancel = new InputEvent 
        { 
            Key = ConsoleKey.Escape, 
            Modifiers = KeyModifiers.None 
        };
        
        public static readonly InputEvent MoveUp = new InputEvent 
        { 
            Key = ConsoleKey.UpArrow, 
            Modifiers = KeyModifiers.None 
        };
        
        public static readonly InputEvent MoveDown = new InputEvent 
        { 
            Key = ConsoleKey.DownArrow, 
            Modifiers = KeyModifiers.None 
        };
        
        public static readonly InputEvent MoveLeft = new InputEvent 
        { 
            Key = ConsoleKey.LeftArrow, 
            Modifiers = KeyModifiers.None 
        };
        
        public static readonly InputEvent MoveRight = new InputEvent 
        { 
            Key = ConsoleKey.RightArrow, 
            Modifiers = KeyModifiers.None 
        };
    }
}