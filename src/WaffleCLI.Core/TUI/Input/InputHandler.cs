using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Core.TUI.Input
{
    /// <summary>
    /// Fixed InputHandler with proper input routing
    /// </summary>
    public class InputHandler : IInputHandler
    {
        private readonly FocusManager _focusManager;
        private readonly KeyBindingManager _keyBindingManager;
        private bool _isRunning = true;
        private DateTime _lastInputLog = DateTime.MinValue;

        public InputHandler(FocusManager focusManager, KeyBindingManager keyBindingManager)
        {
            _focusManager = focusManager;
            _keyBindingManager = keyBindingManager;
        }

        public void ProcessInput()
        {
            if (!_isRunning || !Console.KeyAvailable) return;

            try
            {
                var keyInfo = Console.ReadKey(intercept: true);
                var inputEvent = CreateInputEvent(keyInfo);

                // Log important inputs immediately
                if (inputEvent.Key == ConsoleKey.Enter || inputEvent.Key == ConsoleKey.Spacebar || 
                    inputEvent.Key == ConsoleKey.Tab || inputEvent.Key == ConsoleKey.Escape)
                {
                    Infrastructure.Logging.TuiLogger.LogDebug($"Important input: {inputEvent.Key} (Modifiers: {inputEvent.Modifiers})");
                }

                // First, try global hotkeys
                if (_keyBindingManager.TryHandleGlobalHotkey(inputEvent))
                {
                    Infrastructure.Logging.TuiLogger.LogDebug($"Global hotkey handled: {inputEvent.Key}");
                    return;
                }

                // Then, try focused component
                if (_focusManager.CurrentFocus is IFocusable focused && focused.IsEnabled)
                {
                    Infrastructure.Logging.TuiLogger.LogDebug($"Routing input to focused component: {focused.Id}");
                    if (focused.HandleInput(inputEvent))
                    {
                        Infrastructure.Logging.TuiLogger.LogDebug($"Input handled by focused component: {focused.Id}");
                        return;
                    }
                    else
                    {
                        Infrastructure.Logging.TuiLogger.LogDebug($"Focused component did not handle input: {focused.Id}");
                    }
                }
                else
                {
                    Infrastructure.Logging.TuiLogger.LogDebug($"No focused component or component disabled");
                }

                // Finally, handle navigation keys
                HandleNavigationKeys(inputEvent);
            }
            catch (Exception ex)
            {
                Infrastructure.Logging.TuiLogger.LogError("Input processing failed", ex);
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
            if (!_focusManager.IsEnabled) return;

            bool handled = false;
            
            switch (inputEvent.Key)
            {
                case ConsoleKey.Tab:
                    if (inputEvent.Modifiers.HasFlag(KeyModifiers.Shift))
                    {
                        handled = _focusManager.MoveFocusBackward();
                        Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved backward: {handled}");
                    }
                    else
                    {
                        handled = _focusManager.MoveFocusForward();
                        Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved forward: {handled}");
                    }
                    break;
                    
                case ConsoleKey.UpArrow:
                    handled = _focusManager.MoveFocus(Direction.Up);
                    Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved up: {handled}");
                    break;
                    
                case ConsoleKey.DownArrow:
                    handled = _focusManager.MoveFocus(Direction.Down);
                    Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved down: {handled}");
                    break;
                    
                case ConsoleKey.LeftArrow:
                    handled = _focusManager.MoveFocus(Direction.Left);
                    Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved left: {handled}");
                    break;
                    
                case ConsoleKey.RightArrow:
                    handled = _focusManager.MoveFocus(Direction.Right);
                    Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved right: {handled}");
                    break;
            }

            if (handled)
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Navigation handled: {inputEvent.Key}");
            }
        }
    }
}