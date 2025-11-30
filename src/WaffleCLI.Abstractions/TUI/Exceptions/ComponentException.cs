namespace WaffleCLI.Abstractions.TUI.Exceptions
{
    /// <summary>
    /// Exception for component-related errors
    /// </summary>
    public class ComponentException : TuiException
    {
        public string ComponentId { get; }
        
        public ComponentException(string componentId, string message) 
            : base($"Component {componentId}: {message}")
        {
            ComponentId = componentId;
        }
        
        public ComponentException(string componentId, string message, Exception inner) 
            : base($"Component {componentId}: {message}", inner)
        {
            ComponentId = componentId;
        }
    }
}