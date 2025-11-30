using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Core.TUI.Input;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

// Enable detailed logging
TuiLogger.EnableLogging = true;
TuiLogger.LogFile = "tui-debug.log";
TuiLogger.ClearLog();

TuiLogger.LogInfo("=== WaffleCLI TUI Framework Startup ===");

try
{
    Console.WriteLine("🚀 Initializing WaffleCLI TUI Framework...");
    TuiLogger.LogInfo("Application starting");
    
    // Use in-memory settings to avoid file system issues during development
    Console.WriteLine("📝 Loading settings...");
    var settingsManager = new SettingsManager("tui-settings.json");
    
    // Don't try to create settings file if it fails - use defaults
    if (!File.Exists("tui-settings.json"))
    {
        TuiLogger.LogInfo("Settings file not found, using defaults");
        Console.WriteLine("⚙️  Using default settings (no config file)");
    }
    else
    {
        Console.WriteLine("✅ Settings loaded successfully");
    }

    var settings = settingsManager.Settings;

    Console.WriteLine("🔨 Building TUI application...");
    TuiLogger.LogInfo("Building TuiApplication");
    
    var app = new TuiApplicationBuilder()
        .ConfigureServices(services =>
        {
            TuiLogger.LogInfo("Configuring services");
            services.AddSingleton(settingsManager);
            
            // Apply configuration
            services.AddSingleton<ITuiConfiguration>(new TuiConfiguration
            {
                DefaultTheme = settings.Theme,
                FrameRate = settings.FrameRate,
                EnableDoubleBuffering = settings.EnableDoubleBuffering,
                EnableInputLogging = settings.EnableInputLogging
            });
        })
        .UseRootComponent<DemoApp>()
        .Build();

    Console.WriteLine("✅ Application built successfully!");
    Console.WriteLine("\n🎮 Controls:");
    Console.WriteLine("   • Tab/Shift+Tab - Navigate between components");
    Console.WriteLine("   • Arrow keys - Navigate in lists");
    Console.WriteLine("   • Enter/Space - Activate buttons");
    Console.WriteLine("   • Type - Input text in text boxes");
    Console.WriteLine("   • Esc - Exit application");
    Console.WriteLine("\nPress any key to start the application...");
    
    TuiLogger.LogInfo("Application ready, waiting for user input");
    Console.ReadKey(true);
    
    TuiLogger.LogInfo("Starting application main loop");
    Console.Clear(); // Clear the startup messages
    
    // Run the application
    app.Run();
    
    TuiLogger.LogInfo("Application exited normally");
}
catch (Exception ex)
{
    TuiLogger.LogError("Application failed during startup", ex);
    Console.Clear();
    Console.WriteLine($"💥 Application startup error: {ex.Message}");
    Console.WriteLine($"📋 Details logged to: {Path.GetFullPath("tui-debug.log")}");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}

public class DemoApp : Panel
{
    private readonly Button _button;
    private readonly TextBox _textBox;
    private readonly ListBox _listBox;
    private readonly Label _statusLabel;
    private readonly Label _header;
    private readonly Label _instructions;
    private int _clickCount = 0;

    public DemoApp() : base("mainApp")
    {
        TuiLogger.LogInfo("DemoApp constructor started");
        
        // Safe console dimensions with fallback
        try
        {
            Width = Console.WindowWidth;
            Height = Console.WindowHeight;
            TuiLogger.LogInfo($"Console dimensions: {Width}x{Height}");
        }
        catch (Exception ex)
        {
            TuiLogger.LogError("Failed to get console dimensions, using defaults", ex);
            Width = 80;
            Height = 24;
        }

        // Ensure minimum dimensions
        Width = Math.Max(40, Width);
        Height = Math.Max(20, Height);

        TuiLogger.LogInfo("Creating UI components");

        // Create header
        _header = new Label("header")
        {
            X = 2,
            Y = 1,
            Width = Width - 4,
            Height = 1,
            Text = "🐹 WaffleCLI TUI Framework Demo 🐹",
            Colors = new WaffleCLI.Abstractions.TUI.Rendering.Enums.ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue)
        };

        // Create demo button
        _button = new Button("demoButton")
        {
            X = 2,
            Y = 3,
            Width = 20,
            Height = 3,
            Text = "Click me!",
            OnClick = HandleButtonClick
        };

        // Create text box
        _textBox = new TextBox("demoTextBox")
        {
            X = 2,
            Y = 7,
            Width = 30,
            Height = 1,
            Placeholder = "Enter text here...",
            MaxLength = 50
        };

        // Create list box
        _listBox = new ListBox("demoListBox")
        {
            X = 2,
            Y = 9,
            Width = 30,
            Height = 8,
            OnSelectionChanged = HandleListSelection
        };

        // Populate list with sample items
        for (int i = 1; i <= 15; i++)
        {
            _listBox.Items.Add($"Sample Item {i}");
        }

        // Status label
        _statusLabel = new Label("statusLabel")
        {
            X = 2,
            Y = 18,
            Width = Width - 4,
            Height = 1,
            Text = "✅ Framework initialized! Use Tab to navigate, Esc to exit."
        };

        // Instructions
        _instructions = new Label("instructions")
        {
            X = 2,
            Y = 20,
            Width = Width - 4,
            Height = 3,
            Text = "Controls: Tab=Navigate, Arrows=Lists, Enter=Buttons, Type=Text, Esc=Exit"
        };

        TuiLogger.LogInfo("Adding components to panel");

        // Add all components to the panel
        AddChild(_header);
        AddChild(_button);
        AddChild(_textBox);
        AddChild(_listBox);
        AddChild(_statusLabel);
        AddChild(_instructions);

        TuiLogger.LogInfo("DemoApp constructor completed");
    }

    private void HandleButtonClick()
    {
        _clickCount++;
        var text = string.IsNullOrEmpty(_textBox.Text) ? "<empty>" : _textBox.Text;
        _statusLabel.Text = $"🎉 Button clicked {_clickCount} times! Text: '{text}'";
        
        if (!string.IsNullOrEmpty(_textBox.Text))
        {
            _listBox.Items.Add($"📝 User entry: {_textBox.Text}");
            _textBox.Text = string.Empty;
        }
    }

    private void HandleListSelection(int index)
    {
        if (index >= 0 && index < _listBox.Items.Count)
        {
            var item = _listBox.Items[index];
            _statusLabel.Text = $"🔍 Selected: {item} (index {index})";
        }
    }

    public override void Render(WaffleCLI.Abstractions.TUI.Rendering.IRenderEngine renderEngine)
    {
        TuiLogger.LogDebug($"DemoApp.Render called - Visible: {IsVisible}, Children: {Children.Count}");
        base.Render(renderEngine);
    }
}