using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Input
{
    /// <summary>
    /// Input handler with focus management and hotkey support
    /// </summary>
    public class InputHandler : IInputHandler
    {
        private readonly FocusManager _focusManager;
        private readonly KeyBindingManager _keyBindingManager;
        private bool _isRunning = true;

        public InputHandler(FocusManager focusManager, KeyBindingManager keyBindingManager)
        {
            _focusManager = focusManager;
            _keyBindingManager = keyBindingManager;
        }

        public void ProcessInput()
        {
            if (!Console.KeyAvailable) return;

            try
            {
                var keyInfo = Console.ReadKey(intercept: true);
                var inputEvent = CreateInputEvent(keyInfo);

                // First, try global hotkeys
                if (_keyBindingManager.TryHandleGlobalHotkey(inputEvent))
                {
                    return;
                }

                // Then, try focused component
                if (_focusManager.CurrentFocus is IFocusable focused)
                {
                    if (focused.HandleInput(inputEvent))
                    {
                        return;
                    }
                }

                // Finally, handle navigation keys
                HandleNavigationKeys(inputEvent);
            }
            catch (Exception ex)
            {
                throw new TuiException("Input processing failed", ex);
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private InputEvent CreateInputEvent(ConsoleKeyInfo keyInfo)
        {
            return new InputEvent
            {
                Key = keyInfo.Key,
                Character = keyInfo.KeyChar,
                Modifiers = GetModifiers(keyInfo),
                Timestamp = DateTime.Now
            };
        }

        private KeyModifiers GetModifiers(ConsoleKeyInfo keyInfo)
        {
            var modifiers = KeyModifiers.None;
            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
                modifiers |= KeyModifiers.Control;
            if ((keyInfo.Modifiers & ConsoleModifiers.Alt) != 0)
                modifiers |= KeyModifiers.Alt;
            if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
                modifiers |= KeyModifiers.Shift;
            return modifiers;
        }

        private void HandleNavigationKeys(InputEvent inputEvent)
        {
            switch (inputEvent.Key)
            {
                case ConsoleKey.Tab:
                    if (inputEvent.Modifiers.HasFlag(KeyModifiers.Shift))
                        _focusManager.MoveFocusBackward();
                    else
                        _focusManager.MoveFocusForward();
                    break;
                    
                case ConsoleKey.UpArrow:
                    _focusManager.MoveFocus(Direction.Up);
                    break;
                    
                case ConsoleKey.DownArrow:
                    _focusManager.MoveFocus(Direction.Down);
                    break;
                    
                case ConsoleKey.LeftArrow:
                    _focusManager.MoveFocus(Direction.Left);
                    break;
                    
                case ConsoleKey.RightArrow:
                    _focusManager.MoveFocus(Direction.Right);
                    break;
            }
        }
    }
}