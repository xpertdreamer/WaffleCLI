using WaffleCLI.Abstractions.TUI.Configuration;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// TUI configuration implementation
    /// </summary>
    public class TuiConfiguration : ITuiConfiguration
    {
        public string DefaultTheme { get; set; } = "default";
        public int FrameRate { get; set; } = 60;
        public bool EnableDoubleBuffering { get; set; } = true;
        public bool EnableInputLogging { get; set; } = false;
        public Dictionary<string, object> ComponentDefaults { get; set; } = new();

        public TuiConfiguration()
        {
            // Set default component configurations
            ComponentDefaults["Button.Width"] = 12;
            ComponentDefaults["Button.Height"] = 3;
            ComponentDefaults["TextBox.Width"] = 20;
            ComponentDefaults["TextBox.Height"] = 1;
            ComponentDefaults["ListBox.Width"] = 30;
            ComponentDefaults["ListBox.Height"] = 10;
        }
    }
}