using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Input;

namespace WaffleCLI.Core.TUI.Components.Base
{
    /// <summary>
    /// Enhanced base class for focusable components with proper state change handling
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
                    Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} focus changed to: {value}");
                    
                    // Force visual update when focus changes
                    RequestVisualUpdate();
                    
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
            Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} received focus");
        }

        public virtual void OnBlur()
        {
            // Base implementation - can be overridden
            Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} lost focus");
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
        
        protected virtual bool HandleConfirm() 
        {
            Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} handled confirm action");
            RequestVisualUpdate();
            return false;
        }
        
        protected virtual bool HandleCancel() 
        {
            Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} handled cancel action");
            RequestVisualUpdate();
            return false;
        }
        
        protected void RequestVisualUpdate()
        {
            // This method can be used by derived classes to request visual updates
            // In a more advanced implementation, this could trigger invalidation events
            Infrastructure.Logging.TuiLogger.LogDebug($"Component {Id} requested visual update");
        }
    }
}