namespace WaffleCLI.Abstractions.TUI.Configuration
{
    /// <summary>
    /// Component-specific configuration
    /// </summary>
    public class ComponentConfiguration
    {
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<ComponentConfiguration> Children { get; set; } = new();
    }
}