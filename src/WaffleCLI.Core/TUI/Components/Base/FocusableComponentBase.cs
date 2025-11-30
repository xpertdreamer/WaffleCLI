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
                    // Log only once to avoid duplication
                    Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} focus changed to: {value}");
                    
                    if (value) 
                        OnFocus();
                    else 
                        OnBlur();
                }
            }
        }

        protected FocusableComponentBase(string id) : base(id)
        {
        }

        public virtual void OnFocus()
        {
            // Base implementation - can be overridden
        }

        public virtual void OnBlur()
        {
            // Base implementation - can be overridden
        }

        public abstract bool HandleInput(InputEvent inputEvent);
        
        protected virtual bool HandleCommonNavigation(InputEvent inputEvent)
        {
            // Handle Tab navigation - let FocusManager handle this
            if (inputEvent.Key == ConsoleKey.Tab)
            {
                return false;
            }
            
            // Handle Enter as confirm
            if (inputEvent.Key == ConsoleKey.Enter)
            {
                return HandleConfirm();
            }
            
            // Handle Escape as cancel
            if (inputEvent.Key == ConsoleKey.Escape)
            {
                return HandleCancel();
            }
            
            return false;
        }
        
        protected virtual bool HandleConfirm() => false;
        protected virtual bool HandleCancel() => false;
    }
}