using WaffleCLI.Core.TUI.Elements;

namespace WaffleCLI.Core.TUI.Screens;

public class AlternativeDemoScreen : BasicTuiScreen
{
    public override string Title => "Alternative Demo Screen";

    public override Task InitializeAsync()
    {
        var title = new TextElement
        {
            X = 10,
            Y = 3,
            Width = 40,
            Text = "This is ALTERNATIVE demo screen!",
            Color = ConsoleColor.Red,
            HasBorder = true
        };

        var message = new TextElement
        {
            X = 10,
            Y = 6,
            Width = 50,
            Text = "If you see this, UseStartScreen is working!",
            Color = ConsoleColor.Green
        };

        var button = new ButtonElement
        {
            X = 10,
            Y = 9,
            Width = 25,
            Text = "Alternative Button",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkMagenta
        };

        button.Clicked += () =>
        {
            var result = new TextElement
            {
                X = 10,
                Y = 13,
                Width = 35,
                Text = "Alternative button clicked!",
                Color = ConsoleColor.Cyan,
                HasBorder = true
            };
            AddElement(result);
        };

        AddElement(title);
        AddElement(message);
        AddElement(button);

        return Task.CompletedTask;
    }
}