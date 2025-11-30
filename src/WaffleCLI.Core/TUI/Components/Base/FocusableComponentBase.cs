using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Input;

namespace WaffleCLI.Core.TUI.Components.Base
{
    /// <summary>
    /// Base class for focusable components
    /// </summary>
    public abstract class FocusableComponentBase : ComponentBase, IFocusable
    {
        private bool _hasFocus = false;

        public bool HasFocus 
        { 
            get => _hasFocus;
            set
            {
                if (_hasFocus != value)
                {
                    _hasFocus = value;
                    if (value) OnFocus();
                    else OnBlur();
                }
            }
        }

        protected FocusableComponentBase(string id) : base(id)
        {
        }

        public virtual void OnFocus()
        {
            // Can be overridden by derived classes
        }

        public virtual void OnBlur()
        {
            // Can be overridden by derived classes
        }

        public abstract bool HandleInput(InputEvent inputEvent);
        
        protected virtual bool HandleCommonNavigation(InputEvent inputEvent)
        {
            switch (inputEvent.Key)
            {
                case ConsoleKey.Tab:
                    if (inputEvent.Modifiers.HasFlag(KeyModifiers.Shift))
                    {
                        // Focus previous - handled by FocusManager
                        return false;
                    }
                    else
                    {
                        // Focus next - handled by FocusManager
                        return false;
                    }
                    
                case ConsoleKey.Enter:
                    // Confirm action
                    return HandleConfirm();
                    
                case ConsoleKey.Escape:
                    // Cancel action
                    return HandleCancel();
            }
            
            return false;
        }
        
        protected virtual bool HandleConfirm() => false;
        protected virtual bool HandleCancel() => false;
    }
}