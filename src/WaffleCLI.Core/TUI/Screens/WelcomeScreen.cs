using WaffleCLI.Core.TUI.Elements;

namespace WaffleCLI.Core.TUI.Screens;

public class WelcomeScreen : BasicTuiScreen
{
    public override string Title => "Welcome to WaffleTUI";

    public override Task InitializeAsync()
    {
        var welcomeText = new TextElement
        {
            X = 10,
            Y = 5,
            Width = 40,
            Text = "Hello, User!",
            Color = ConsoleColor.Green
        };

        var instructionText = new TextElement
        {
            X = 10,
            Y = 7,
            Width = 50,
            Text = "This is your first WaffleTUI app!",
            Color = ConsoleColor.Yellow
        };

        var hintText = new TextElement
        {
            X = 10,
            Y = 9, 
            Width = 60,
            Text = "Press any key to continue...",
            Color = ConsoleColor.Gray
        };
        
        AddElement(welcomeText);
        AddElement(instructionText);
        AddElement(hintText);
        
        return Task.CompletedTask;
    }

    public override Task HandleInputAsync(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo is { Key: ConsoleKey.Q, Modifiers: ConsoleModifiers.Control })
        {
            Environment.Exit(0);
        }
        else
        {
            // Will add navigation here later
            // For now just leave it like this
            
            Console.Clear();
            Console.WriteLine("Goodbye");
            Environment.Exit(0);
        }
        
        return Task.CompletedTask;
    }
}