using WaffleCLI.Core.TUI.Elements;

namespace WaffleCLI.Core.TUI.Screens;

public class WelcomeScreen : BasicTuiScreen
{
    public override string Title => "Welcome to WaffleTUI";

    public override Task InitializeAsync()
    {
        var title = new TextElement
        {
            X = 5,
            Y = 2,
            Width = 40,
            Text = "Welcome to WaffleTUI!",
            Color = ConsoleColor.Green,
            HasBorder = true
        };

        var instructionText = new TextElement
        {
            X = 10,
            Y = 7,
            Width = 50,
            Text = "This is your first WaffleTUI app!",
            Color = ConsoleColor.Yellow
        };

        var instruction2 = new TextElement
        {
            X = 5,
            Y = 6,
            Width = 50,
            Text = "This is a simple TUI Framework",
            Color = ConsoleColor.Yellow
        };
        
        var instruction3 = new TextElement
        {
            X = 5,
            Y = 8,
            Width = 50,
            Text = "Window size: " + Console.WindowWidth + "x" + Console.WindowHeight,
            Color = ConsoleColor.Gray
        };

        var button = new ButtonElement
        {
            X = 5,
            Y = 10,
            Width = 20,
            Text = "Test Button",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkBlue
        };
        
        button.Clicked += () =>
        {
            var message = new TextElement
            {
                X = 5,
                Y = 14,
                Width = 30,
                Text = "Button was clicked!",
                Color = ConsoleColor.Yellow,
                HasBorder = true
            };
            AddElement(message);
        };
        
        var frame = new TextElement
        {
            X = 3,
            Y = 1,
            Width = 60,
            Height = 15,
            Text = "",
            HasBorder = true,
            BorderColor = ConsoleColor.Blue
        };
        
        AddElement(frame);
        AddElement(title);
        AddElement(instructionText);
        AddElement(instruction2);
        AddElement(instruction3);
        AddElement(button);
        
        return Task.CompletedTask;
    }
}