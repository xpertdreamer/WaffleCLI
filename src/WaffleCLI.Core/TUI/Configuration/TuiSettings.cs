using System.Text.Json.Serialization;

namespace WaffleCLI.Core.TUI.Configuration
{
    /// <summary>
    /// JSON-serializable settings for TUI framework
    /// </summary>
    public class TuiSettings
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "default";

        [JsonPropertyName("frameRate")]
        public int FrameRate { get; set; } = 60;

        [JsonPropertyName("enableDoubleBuffering")]
        public bool EnableDoubleBuffering { get; set; } = true;

        [JsonPropertyName("enableInputLogging")]
        public bool EnableInputLogging { get; set; } = false;

        [JsonPropertyName("components")]
        public ComponentSettings Components { get; set; } = new();

        [JsonPropertyName("colors")]
        public ColorSettings Colors { get; set; } = new();
    }

    public class ComponentSettings
    {
        [JsonPropertyName("button")]
        public ButtonSettings Button { get; set; } = new();

        [JsonPropertyName("textBox")]
        public TextBoxSettings TextBox { get; set; } = new();

        [JsonPropertyName("listBox")]
        public ListBoxSettings ListBox { get; set; } = new();
    }

    public class ButtonSettings
    {
        [JsonPropertyName("defaultWidth")]
        public int DefaultWidth { get; set; } = 12;

        [JsonPropertyName("defaultHeight")]
        public int DefaultHeight { get; set; } = 3;

        [JsonPropertyName("normalColors")]
        public string NormalColors { get; set; } = "primary";

        [JsonPropertyName("focusColors")]
        public string FocusColors { get; set; } = "focus";
    }

    public class TextBoxSettings
    {
        [JsonPropertyName("defaultWidth")]
        public int DefaultWidth { get; set; } = 20;

        [JsonPropertyName("defaultHeight")]
        public int DefaultHeight { get; set; } = 1;

        [JsonPropertyName("maxLength")]
        public int MaxLength { get; set; } = 256;
    }

    public class ListBoxSettings
    {
        [JsonPropertyName("defaultWidth")]
        public int DefaultWidth { get; set; } = 30;

        [JsonPropertyName("defaultHeight")]
        public int DefaultHeight { get; set; } = 10;
    }

    public class ColorSettings
    {
        [JsonPropertyName("primary")]
        public string Primary { get; set; } = "White:DarkBlue";

        [JsonPropertyName("secondary")]
        public string Secondary { get; set; } = "Gray:Black";

        [JsonPropertyName("focus")]
        public string Focus { get; set; } = "Black:White";

        [JsonPropertyName("success")]
        public string Success { get; set; } = "Green:Black";

        [JsonPropertyName("warning")]
        public string Warning { get; set; } = "Yellow:Black";

        [JsonPropertyName("error")]
        public string Error { get; set; } = "Red:Black";
    }
}