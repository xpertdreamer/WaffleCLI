using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

// Enable logging but use quiet mode to avoid console spam
TuiLogger.EnableLogging = true;
TuiLogger.QuietMode = true; // This is the key setting - no console output from logs
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
    
    var app = new TuiApplicationBuilder()
        .ConfigureServices(services =>
        {
            TuiLogger.LogInfo("Configuring services");
            services.AddSingleton(settingsManager);
            
            // Apply configuration with optimized settings
            services.AddSingleton<ITuiConfiguration>(new TuiConfiguration
            {
                DefaultTheme = settings.Theme,
                FrameRate = 30, // Reduced for better performance
                EnableDoubleBuffering = settings.EnableDoubleBuffering,
                EnableInputLogging = false // Disable input logging to reduce spam
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
    Console.Clear();
    
    // Run the application
    app.Run();
    
    TuiLogger.LogInfo("Application exited normally");
    
    // Show exit message
    Console.WriteLine("👋 Application finished. Check tui-debug.log for details.");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
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
    private readonly Label _debugLabel;
    private int _clickCount = 0;
    private string _lastAction = "App started";

    public DemoApp() : base("mainApp")
    {
        TuiLogger.LogInfo("DemoApp constructor started");
        
        // Safe console dimensions with fallback and validation
        try
        {
            Width = Math.Max(40, Console.WindowWidth);
            Height = Math.Max(20, Console.WindowHeight);
            TuiLogger.LogInfo($"DemoApp dimensions: {Width}x{Height}");
        }
        catch (Exception ex)
        {
            TuiLogger.LogError("Failed to get console dimensions, using defaults", ex);
            Width = 80;
            Height = 24;
        }

        BackgroundColors = new WaffleCLI.Abstractions.TUI.Rendering.Enums.ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);
        Border = WaffleCLI.Abstractions.TUI.Rendering.Enums.BorderStyle.Double;
        BorderColors = new WaffleCLI.Abstractions.TUI.Rendering.Enums.ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);

        TuiLogger.LogInfo("Creating UI components");

        // Create header with safe positioning
        _header = new Label("header")
        {
            X = 2,
            Y = 1,
            Width = Math.Max(10, Width - 4),
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
            OnClick = HandleButtonClick,
            NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkYellow)
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

        // Debug label to show current focus
        _debugLabel = new Label("debugLabel")
        {
            X = 35,
            Y = 3,
            Width = 40,
            Height = 1,
            Text = "Debug: No focus",
            Colors = new WaffleCLI.Abstractions.TUI.Rendering.Enums.ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue)
        };

        // Status label
        _statusLabel = new Label("statusLabel")
        {
            X = 2,
            Y = 18,
            Width = Math.Max(10, Width - 4),
            Height = 1,
            Text = "✅ Framework initialized! Use Tab to navigate, Esc to exit.",
            Colors = new WaffleCLI.Abstractions.TUI.Rendering.Enums.ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue)
        };

        // Instructions
        _instructions = new Label("instructions")
        {
            X = 2,
            Y = 20,
            Width = Math.Max(10, Width - 4),
            Height = 3,
            Text = "Controls: Tab=Navigate, Arrows=Lists, Enter=Buttons, Type=Text, Esc=Exit",
            Colors = new WaffleCLI.Abstractions.TUI.Rendering.Enums.ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue)
        };

        TuiLogger.LogInfo("Adding components to panel");

        // Add all components to the panel
        AddChild(_header);
        AddChild(_button);
        AddChild(_textBox);
        AddChild(_listBox);
        AddChild(_debugLabel);
        AddChild(_statusLabel);
        AddChild(_instructions);

        TuiLogger.LogInfo("DemoApp constructor completed");
    }

    private void HandleButtonClick()
    {
        _clickCount++;
        var text = string.IsNullOrEmpty(_textBox.Text) ? "<empty>" : _textBox.Text;
        _statusLabel.Text = $"🎉 Button clicked {_clickCount} times! Text: '{text}'";
        _lastAction = $"Button clicked {_clickCount} times";
        
        TuiLogger.LogInfo($"Button clicked! Count: {_clickCount}, Text: '{text}'");

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
            _lastAction = $"Selected: {item}";
            TuiLogger.LogInfo($"List selection: {item} at index {index}");
        }
    }

    public override void Update()
    {
        // Update debug info
        UpdateDebugInfo();
        base.Update();
    }

    private void UpdateDebugInfo()
    {
        // This is a simplified way to track focus - in a real app you'd inject FocusManager
        string focusInfo = "Focus: ";
        
        if (_button.HasFocus)
            focusInfo = "Focus: Button";
        else if (_textBox.HasFocus)
            focusInfo = "Focus: TextBox";
        else if (_listBox.HasFocus)
            focusInfo = "Focus: ListBox";
        else
            focusInfo = "Focus: None";

        _debugLabel.Text = $"{focusInfo} | Last: {_lastAction}";
    }
}