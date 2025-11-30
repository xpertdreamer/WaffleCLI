using WaffleCLI.Abstractions.TUI.Rendering.Enums;

namespace WaffleCLI.Abstractions.TUI.Configuration
{
    /// <summary>
    /// Theme configuration
    /// </summary>
    public class ThemeConfiguration
    {
        public string Name { get; set; } = "default";
        public Dictionary<string, ColorScheme> ColorSchemes { get; set; } = new();
        public Dictionary<string, BorderStyle> BorderStyles { get; set; } = new();
        
        public ColorScheme GetColorScheme(string schemeName)
        {
            return ColorSchemes.TryGetValue(schemeName, out var scheme) 
                ? scheme 
                : ColorScheme.Default;
        }
        
        public BorderStyle GetBorderStyle(string styleName)
        {
            return BorderStyles.TryGetValue(styleName, out var style)
                ? style
                : BorderStyle.Single;
        }
    }
}