using System.Text.Json.Serialization;

namespace WaffleCLI.Core.TUI.Configuration;

public class AppConfig
{
    [JsonPropertyName("window")]
    public WindowConfig Window { get; set; } = new();

    [JsonPropertyName("rendering")]
    public RenderingConfig Rendering { get; set; } = new();

    [JsonPropertyName("theme")]
    public ThemeConfig Theme { get; set; } = new();

    [JsonPropertyName("input")]
    public InputConfig Input { get; set; } = new();

    [JsonPropertyName("behavior")]
    public BehaviorConfig Behavior { get; set; } = new();

    [JsonPropertyName("plugins")]
    public PluginsConfig Plugins { get; set; } = new();

    [JsonPropertyName("logging")]
    public LoggingConfig Logging { get; set; } = new();
}

public class WindowConfig
{
    [JsonPropertyName("width")] public int Width { get; set; } = 80;
    [JsonPropertyName("height")] public int Height { get; set; } = 25;
    [JsonPropertyName("title")]  public string Title { get; set; } = "WaffleTUI Application";
    [JsonPropertyName("resizable")] public bool Resizable { get; set; } = true;
    [JsonPropertyName("centerOnStart")] public bool CenterOnStart { get; set; } = true;
}

public class RenderingConfig
{
    [JsonPropertyName("doubleBuffering")] public bool DoubleBuffering { get; set; } = true;
    [JsonPropertyName("partialRendering")] public bool PartialRendering { get; set; } = true;
    [JsonPropertyName("targetFps")] public int TargetFps { get; set; } = 60;
    [JsonPropertyName("vsync")] public bool VSync { get; set; } = true;
    [JsonPropertyName("renderStats")] public bool ShowRenderStats { get; set; } = false;
    [JsonPropertyName("optimizeForTerminal")]  public string OptimizeForTerminal { get; set; } = "auto"; // auto, windows, linux, macos
}

public class ThemeConfig
{
    [JsonPropertyName("current")] public string Current { get; set; } = "default";
    [JsonPropertyName("themes")] public Dictionary<string, ThemeDefinition> Themes { get; set; } = new();
    [JsonPropertyName("customThemesPath")] public string CustomThemesPath { get; set; } = "./themes";
}

public class ThemeDefinition
{
    [JsonPropertyName("name")] public string Name { get; set; } = "Default";
    [JsonPropertyName("author")] public string Author { get; set; } = "WaffleTUI";
    [JsonPropertyName("colors")] public ThemeColors Colors { get; set; } = new();
    [JsonPropertyName("borders")] public BorderSettings Borders { get; set; } = new();
    [JsonPropertyName("animations")] public AnimationSettings Animations { get; set; } = new();
}

public class ThemeColors
{
    [JsonPropertyName("primary")] public string Primary { get; set; } = "Cyan";
    [JsonPropertyName("secondary")] public string Secondary { get; set; } = "DarkBlue";
    [JsonPropertyName("accent")] public string Accent { get; set; } = "Yellow";
    [JsonPropertyName("success")] public string Success { get; set; } = "Green";
    [JsonPropertyName("warning")] public string Warning { get; set; } = "Yellow";
    [JsonPropertyName("error")] public string Error { get; set; } = "Red";
    [JsonPropertyName("text")] public string Text { get; set; } = "White";
    [JsonPropertyName("background")] public string Background { get; set; } = "Black";
    [JsonPropertyName("border")] public string Border { get; set; } = "Gray";
}

public class BorderSettings
{
    [JsonPropertyName("style")] public string Style { get; set; } = "Single";
    [JsonPropertyName("roundedCorners")] public bool RoundedCorners { get; set; } = false;
    [JsonPropertyName("thickness")] public int Thickness { get; set; } = 1;
}

public class AnimationSettings
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("duration")] public double Duration { get; set; } = 0.3;
    [JsonPropertyName("easing")] public string Easing { get; set; } = "EaseOut"; // Linear, EaseIn, EaseOut, EaseInOut
}

public class InputConfig
{
    [JsonPropertyName("keyBindings")] public KeyBindingsConfig KeyBindings { get; set; } = new();
    [JsonPropertyName("mouseSupport")] public bool MouseSupport { get; set; } = false;
    [JsonPropertyName("inputBufferSize")] public int InputBufferSize { get; set; } = 10;
    [JsonPropertyName("repeatDelay")] public int RepeatDelay { get; set; } = 500;
    [JsonPropertyName("repeatInterval")] public int RepeatInterval { get; set; } = 50;
}

public class KeyBindingsConfig
{
    [JsonPropertyName("exit")] public string Exit { get; set; } = "Ctrl+Q";
    [JsonPropertyName("navigateNext")] public string NavigateNext { get; set; } = "Tab";
    [JsonPropertyName("navigatePrevious")] public string NavigatePrevious { get; set; } = "Shift+Tab";
    [JsonPropertyName("confirm")] public string Confirm { get; set; } = "Enter";
    [JsonPropertyName("cancel")] public string Cancel { get; set; } = "Escape";
    [JsonPropertyName("screenshot")] public string Screenshot { get; set; } = "F12";
    [JsonPropertyName("toggleStats")] public string ToggleStats { get; set; } = "F11";
}

public class BehaviorConfig
{
    [JsonPropertyName("autoSave")] public bool AutoSave { get; set; } = true;
    [JsonPropertyName("saveInterval")] public int SaveInterval { get; set; } = 30000; // ms
    [JsonPropertyName("confirmExit")] public bool ConfirmExit { get; set; } = true;
    [JsonPropertyName("reloadOnConfigChange")] public bool ReloadOnConfigChange { get; set; } = true;
    [JsonPropertyName("culture")] public string Culture { get; set; } = "en-US";
    [JsonPropertyName("timezone")] public string Timezone { get; set; } = "UTC";
}

public class PluginsConfig
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("pluginsPath")] public string PluginsPath { get; set; } = "./plugins";
    [JsonPropertyName("autoLoad")] public bool AutoLoad { get; set; } = true;
    [JsonPropertyName("allowedExtensions")] public string[] AllowedExtensions { get; set; } = { ".dll", ".so", ".dylib" };
}

public class LoggingConfig
{
    [JsonPropertyName("level")] public string Level { get; set; } = "Warning";
    [JsonPropertyName("file")] public string File { get; set; } = "waffletui.log";
    [JsonPropertyName("maxFileSize")] public long MaxFileSize { get; set; } = 10485760; // 10MB
    [JsonPropertyName("retainedFileCount")] public int RetainedFileCount { get; set; } = 5;
    [JsonPropertyName("consoleOutput")] public bool ConsoleOutput { get; set; } = true;
}