using WaffleCLI.Core.TUI.Elements;

namespace WaffleCLI.Core.TUI.Screens;

public class WelcomeScreen : BasicTuiScreen
{
    public override string Title => "Welcome to WaffleTUI";

    public override Task InitializeAsync()
    {
        var title = new TextElement
        {
            X = 20,
            Y = 2,
            Width = 40,
            Text = "Welcome to WaffleTUI!",
            Color = ConsoleColor.Green,
            HasBorder = true,
            isFocusable = false
        };

        var instructionText = new TextElement
        {
            X = 15,
            Y = 5,
            Width = 50,
            Text = "This is your first WaffleTUI app!",
            Color = ConsoleColor.Yellow,
            isFocusable = false
        };

        var instruction2 = new TextElement
        {
            X = 15,
            Y = 6,
            Width = 50,
            Text = "This is a simple TUI Framework",
            Color = ConsoleColor.Yellow,
            isFocusable = false
        };
        
        var instruction3 = new TextElement
        {
            X = 15,
            Y = 7,
            Width = 50,
            Text = "Window size: " + Console.WindowWidth + "x" + Console.WindowHeight,
            Color = ConsoleColor.Gray,
            isFocusable = false
        };

        var button = new ButtonElement
        {
            X = 30,
            Y = 10,
            Width = 20,
            Text = "Test Button",
            Color = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkBlue,
            isFocusable = true
        };
        
        button.Clicked += () =>
        {
            var existingMessage = _elements.OfType<TextElement>()
                .FirstOrDefault(e => e.Text == "Button was clicked!");
            if (existingMessage != null)
                RemoveElement(existingMessage);
            
            var message = new TextElement
            {
                X = 25,
                Y = 14,
                Width = 30,
                Text = "Button was clicked!",
                Color = ConsoleColor.Yellow,
                HasBorder = true,
                isFocusable = false
            };
            AddElement(message);
        };
        
        AddElement(title);
        AddElement(instructionText);
        AddElement(instruction2);
        AddElement(instruction3);
        AddElement(button);
        
        return Task.CompletedTask;
    }
}