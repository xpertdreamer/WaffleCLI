using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Abstractions.TUI.Exceptions;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// Manages application settings from JSON configuration
    /// </summary>
    public class SettingsManager
    {
        private readonly string _settingsPath;
        private TuiSettings _settings;

        public TuiSettings Settings => _settings;

        // JSON options for .NET 8+ compatibility
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            // Enable reflection-based serialization for .NET 8+
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        public SettingsManager(string settingsPath = null)
        {
            // Use current directory for demo to avoid permission issues
            _settingsPath = settingsPath ?? "tui-settings.json";
            _settings = LoadSettings();
        }

        public TuiSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<TuiSettings>(json, _jsonOptions);
                    
                    return settings ?? CreateDefaultSettings();
                }
                else
                {
                    return CreateDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to load settings from {_settingsPath}", ex);
                return CreateDefaultSettings();
            }
        }

        public bool SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, _jsonOptions);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(_settingsPath, json);
                TuiLogger.LogInfo($"Settings saved to {_settingsPath}");
                return true;
            }
            catch (Exception ex)
            {
                TuiLogger.LogError($"Failed to save settings to {_settingsPath}", ex);
                return false;
            }
        }

        public bool UpdateSettings(TuiSettings newSettings)
        {
            _settings = newSettings;
            return SaveSettings();
        }

        public ColorScheme GetColorScheme(string schemeName)
        {
            return schemeName?.ToLower() switch
            {
                "primary" => ParseColorScheme(_settings.Colors.Primary),
                "secondary" => ParseColorScheme(_settings.Colors.Secondary),
                "focus" => ParseColorScheme(_settings.Colors.Focus),
                "success" => ParseColorScheme(_settings.Colors.Success),
                "warning" => ParseColorScheme(_settings.Colors.Warning),
                "error" => ParseColorScheme(_settings.Colors.Error),
                _ => ColorScheme.Default
            };
        }

        private ColorScheme ParseColorScheme(string colorString)
        {
            if (string.IsNullOrEmpty(colorString))
                return ColorScheme.Default;

            var parts = colorString.Split(':');
            if (parts.Length == 2 && 
                Enum.TryParse<ConsoleColor>(parts[0], out ConsoleColor foreground) &&
                Enum.TryParse<ConsoleColor>(parts[1], out ConsoleColor background))
            {
                return new ColorScheme(foreground, background);
            }

            return ColorScheme.Default;
        }

        private TuiSettings CreateDefaultSettings()
        {
            try
            {
                int defaultWidth = 120;
                int defaultHeight = 35;
                
                try
                {
                    defaultWidth = Math.Max(80, Console.WindowWidth);
                    defaultHeight = Math.Max(25, Console.WindowHeight);
                }
                catch
                {
                    // ignore
                }

                return new TuiSettings
                {
                    Theme = "default",
                    FrameRate = 60,
                    EnableDoubleBuffering = true,
                    EnableInputLogging = false,
                    WindowWidth = defaultWidth,
                    WindowHeight = defaultHeight,
                    Components = new ComponentSettings
                    {
                        Button = new ButtonSettings
                        {
                            DefaultWidth = 12,
                            DefaultHeight = 3,
                            NormalColors = "primary",
                            FocusColors = "focus"
                        },
                        TextBox = new TextBoxSettings
                        {
                            DefaultWidth = 20,
                            DefaultHeight = 1,
                            MaxLength = 256
                        },
                        ListBox = new ListBoxSettings
                        {
                            DefaultWidth = 30,
                            DefaultHeight = 10
                        }
                    },
                    Colors = new ColorSettings
                    {
                        Primary = "White:DarkBlue",
                        Secondary = "Gray:Black",
                        Focus = "Black:White",
                        Success = "Green:Black",
                        Warning = "Yellow:Black",
                        Error = "Red:Black"
                    }
                };
            }
            catch (Exception ex)
            {
                TuiLogger.LogError("Failed to create default settings", ex);
                return new TuiSettings();
            }
        }

        public bool CreateDefaultSettingsFile()
        {
            _settings = CreateDefaultSettings();
            return SaveSettings();
        }

        public string GetSettingsPath()
        {
            return _settingsPath;
        }
    }
}