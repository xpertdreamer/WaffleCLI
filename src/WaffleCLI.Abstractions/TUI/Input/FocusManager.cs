using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Abstractions.TUI.Input
{
    /// <summary>
    /// Manages focus between focusable components
    /// </summary>
    public class FocusManager
    {
        private readonly List<IFocusable> _focusableComponents = new();
        private int _currentFocusIndex = -1;

        public IFocusable? CurrentFocus => _currentFocusIndex >= 0 ? _focusableComponents[_currentFocusIndex] : null;
        public IReadOnlyList<IFocusable> FocusableComponents => _focusableComponents.AsReadOnly();

        public event Action<IFocusable?>? FocusChanged;

        public void RegisterFocusable(IFocusable component)
        {
            if (!_focusableComponents.Contains(component))
            {
                _focusableComponents.Add(component);
                
                // Auto-focus first component if none focused
                if (_currentFocusIndex == -1 && _focusableComponents.Count > 0)
                {
                    SetFocus(0);
                }
            }
        }

        public void UnregisterFocusable(IFocusable component)
        {
            int index = _focusableComponents.IndexOf(component);
            if (index >= 0)
            {
                _focusableComponents.RemoveAt(index);
                
                if (index == _currentFocusIndex)
                {
                    _currentFocusIndex = -1;
                    MoveFocusForward(); // Try to focus next component
                }
                else if (index < _currentFocusIndex)
                {
                    _currentFocusIndex--;
                }
            }
        }

        public void MoveFocusForward()
        {
            if (_focusableComponents.Count == 0) return;
            
            var newIndex = (_currentFocusIndex + 1) % _focusableComponents.Count;
            SetFocus(newIndex);
        }

        public void MoveFocusBackward()
        {
            if (_focusableComponents.Count == 0) return;
            
            var newIndex = _currentFocusIndex - 1;
            if (newIndex < 0) newIndex = _focusableComponents.Count - 1;
            SetFocus(newIndex);
        }

        public void MoveFocus(Direction direction)
        {
            // Simple implementation - just move forward
            // Could be enhanced with spatial navigation
            MoveFocusForward();
        }

        public void SetFocus(IFocusable component)
        {
            int index = _focusableComponents.IndexOf(component);
            if (index >= 0)
            {
                SetFocus(index);
            }
        }

        private void SetFocus(int newIndex)
        {
            if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
            {
                _focusableComponents[_currentFocusIndex].HasFocus = false;
                _focusableComponents[_currentFocusIndex].OnBlur();
            }

            _currentFocusIndex = newIndex;

            if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
            {
                _focusableComponents[_currentFocusIndex].HasFocus = true;
                _focusableComponents[_currentFocusIndex].OnFocus();
            }
            
            FocusChanged?.Invoke(CurrentFocus);
        }
    }
}