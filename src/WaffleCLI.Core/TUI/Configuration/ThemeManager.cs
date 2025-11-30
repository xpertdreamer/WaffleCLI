using System.Text.Json;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// Manages themes and color schemes
    /// </summary>
    public class ThemeManager
    {
        private readonly Dictionary<string, ThemeConfiguration> _themes = new();
        private string _currentThemeName = "default";

        public ThemeConfiguration CurrentTheme => _themes[_currentThemeName];

        public ThemeManager()
        {
            LoadDefaultThemes();
        }

        public void LoadThemeFromFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var theme = JsonSerializer.Deserialize<ThemeConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (theme != null && !string.IsNullOrEmpty(theme.Name))
                {
                    _themes[theme.Name] = theme;
                }
            }
            catch (Exception ex)
            {
                throw new TuiException($"Failed to load theme from {filePath}", ex);
            }
        }

        public void SetTheme(string themeName)
        {
            if (_themes.ContainsKey(themeName))
            {
                _currentThemeName = themeName;
            }
            else
            {
                throw new ArgumentException($"Theme '{themeName}' not found");
            }
        }

        public void RegisterTheme(ThemeConfiguration theme)
        {
            _themes[theme.Name] = theme;
        }

        public IReadOnlyCollection<string> GetAvailableThemes()
        {
            return _themes.Keys;
        }

        private void LoadDefaultThemes()
        {
            // Default theme
            var defaultTheme = new ThemeConfiguration
            {
                Name = "default",
                ColorSchemes = new Dictionary<string, ColorScheme>
                {
                    ["primary"] = ColorScheme.Primary,
                    ["secondary"] = ColorScheme.Secondary,
                    ["success"] = ColorScheme.Success,
                    ["warning"] = ColorScheme.Warning,
                    ["error"] = ColorScheme.Error,
                    ["focus"] = ColorScheme.Focus,
                    ["default"] = ColorScheme.Default
                }
            };

            // Dark theme
            var darkTheme = new ThemeConfiguration
            {
                Name = "dark",
                ColorSchemes = new Dictionary<string, ColorScheme>
                {
                    ["primary"] = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkGray),
                    ["secondary"] = new ColorScheme(ConsoleColor.DarkGray, ConsoleColor.Black),
                    ["focus"] = new ColorScheme(ConsoleColor.Black, ConsoleColor.White),
                    ["default"] = new ColorScheme(ConsoleColor.White, ConsoleColor.Black)
                }
            };

            RegisterTheme(defaultTheme);
            RegisterTheme(darkTheme);
        }
    }
}