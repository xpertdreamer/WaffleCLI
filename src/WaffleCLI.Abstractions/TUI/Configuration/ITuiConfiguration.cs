namespace WaffleCLI.Abstractions.TUI.Configuration
{
    /// <summary>
    /// TUI configuration interface
    /// </summary>
    public interface ITuiConfiguration
    {
        string DefaultTheme { get; set; }
        int FrameRate { get; set; }
        bool EnableDoubleBuffering { get; set; }
        bool EnableInputLogging { get; set; }
        Dictionary<string, object> ComponentDefaults { get; set; }
    }
}