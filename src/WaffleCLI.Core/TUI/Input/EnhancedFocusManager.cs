using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Input;

namespace WaffleCLI.Core.TUI.Input
{
    /// <summary>
    /// Enhanced focus manager with proper logging
    /// </summary>
    public class EnhancedFocusManager : FocusManager
    {
        public new void RegisterFocusable(IFocusable component)
        {
            if (component == null) return;
            
            if (!FocusableComponents.Contains(component))
            {
                base.RegisterFocusable(component);
                Infrastructure.Logging.TuiLogger.LogDebug($"Registered focusable: {component.Id}, total: {FocusableComponents.Count}");
            }
        }

        public new bool SetFocus(int newIndex)
        {
            bool result = base.SetFocus(newIndex);
            if (result && CurrentFocus != null)
            {
                Infrastructure.Logging.TuiLogger.LogInfo($"Focus moved to: {CurrentFocus.Id} (index: {newIndex})");
            }
            return result;
        }

        public new bool MoveFocusForward()
        {
            bool result = base.MoveFocusForward();
            if (result)
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved forward to index: {base.FocusableComponents.ToList().IndexOf(CurrentFocus!)}");
            }
            return result;
        }

        public new bool MoveFocusBackward()
        {
            bool result = base.MoveFocusBackward();
            if (result)
            {
                Infrastructure.Logging.TuiLogger.LogDebug($"Focus moved backward to index: {base.FocusableComponents.ToList().IndexOf(CurrentFocus!)}");
            }
            return result;
        }
    }
}