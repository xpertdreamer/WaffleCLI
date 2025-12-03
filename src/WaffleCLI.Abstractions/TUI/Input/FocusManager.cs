using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Abstractions.TUI.Input
{
    /// <summary>
    /// Fixed FocusManager without Core dependencies
    /// </summary>
    public class FocusManager
    {
        private readonly List<IFocusable> _focusableComponents = new();
        private int _currentFocusIndex = -1;
        private bool _isEnabled = true;

        public IFocusable? CurrentFocus => _currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count 
            ? _focusableComponents[_currentFocusIndex] 
            : null;
            
        public IReadOnlyList<IFocusable> FocusableComponents => _focusableComponents.AsReadOnly();
        public bool IsEnabled 
        { 
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (!value && CurrentFocus != null)
                {
                    CurrentFocus.HasFocus = false;
                }
            }
        }

        public event Action<IFocusable?>? FocusChanged;

        public void RegisterFocusable(IFocusable component)
        {
            if (component == null) return;
            
            if (!_focusableComponents.Contains(component))
            {
                _focusableComponents.Add(component);
                
                // Auto-focus first component if none focused and manager is enabled
                if (_currentFocusIndex == -1 && _focusableComponents.Count > 0 && _isEnabled)
                {
                    SetFocus(0);
                }
            }
        }

        public void UnregisterFocusable(IFocusable component)
        {
            if (component == null) return;
            
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

        public bool MoveFocusForward()
        {
            if (!_isEnabled || _focusableComponents.Count == 0) return false;
            
            var newIndex = (_currentFocusIndex + 1) % _focusableComponents.Count;
            return SetFocus(newIndex);
        }

        public bool MoveFocusBackward()
        {
            if (!_isEnabled || _focusableComponents.Count == 0) return false;
            
            var newIndex = _currentFocusIndex - 1;
            if (newIndex < 0) newIndex = _focusableComponents.Count - 1;
            return SetFocus(newIndex);
        }

        public bool MoveFocus(Direction direction)
        {
            if (!_isEnabled || _focusableComponents.Count == 0) return false;
            
            // Actual implementation for directional movement
            // Note: This is a simple implementation. For complex layouts,
            // you would need spatial awareness of component positions.
            switch (direction)
            {
                case Direction.Up:
                    // In a real implementation, you would find the component above
                    // For now, fall back to forward movement
                    return MoveFocusForward();
                    
                case Direction.Down:
                    // In a real implementation, you would find the component below
                    return MoveFocusForward();
                    
                case Direction.Left:
                    return MoveFocusBackward();
                    
                case Direction.Right:
                    return MoveFocusForward();
                    
                default:
                    return MoveFocusForward();
            }
        }

        public bool SetFocus(IFocusable component)
        {
            if (!_isEnabled || component == null) return false;
            
            int index = _focusableComponents.IndexOf(component);
            if (index >= 0)
            {
                return SetFocus(index);
            }
            return false;
        }

        public bool SetFocus(int newIndex)
        {
            if (!_isEnabled || newIndex < 0 || newIndex >= _focusableComponents.Count) 
                return false;

            try
            {
                // Remove focus from current component
                if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
                {
                    var current = _focusableComponents[_currentFocusIndex];
                    current.HasFocus = false;
                }

                _currentFocusIndex = newIndex;

                // Set focus to new component
                if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
                {
                    var newFocus = _focusableComponents[_currentFocusIndex];
                    newFocus.HasFocus = true;
                }
                
                FocusChanged?.Invoke(CurrentFocus);
                return true;
            }
            catch (Exception ex)
            {
                // Logging would be handled by the implementation
                return false;
            }
        }

        public void ClearFocus()
        {
            if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
            {
                _focusableComponents[_currentFocusIndex].HasFocus = false;
                _currentFocusIndex = -1;
                FocusChanged?.Invoke(null);
            }
        }

        public void Reset()
        {
            ClearFocus();
            _focusableComponents.Clear();
            _currentFocusIndex = -1;
        }
    }
}