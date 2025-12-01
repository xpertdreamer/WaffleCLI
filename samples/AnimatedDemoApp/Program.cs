using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Components.Animated;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Core.TUI.Animations;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

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
        .UseRootComponent<AnimatedDemoApp>()
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

public class AnimatedDemoApp : Panel
{
    private readonly AnimationManager _animationManager;
    private readonly AnimatedButton _animatedButton;
    private readonly Label _statusLabel;
    private readonly Label _fpsLabel;
    private int _clickCount = 0;
    private DateTime _lastFpsUpdate = DateTime.Now;
    private int _frameCount = 0;

    public AnimatedDemoApp(AnimationManager animationManager) : base("animatedApp")
    {
        _animationManager = animationManager;

        Width = 80;
        Height = 24;
        BackgroundColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);

        // Create animated button
        _animatedButton = new AnimatedButton("animButton", _animationManager)
        {
            X = 30,
            Y = 5,
            Width = 20,
            Height = 3,
            Text = "Animated Button!",
            OnClick = OnAnimatedButtonClick
        };

        // Status label
        _statusLabel = new Label("statusLabel")
        {
            X = 25,
            Y = 10,
            Width = 30,
            Height = 1,
            Text = "Click the button to see animations!",
            Colors = new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue)
        };

        // FPS counter
        _fpsLabel = new Label("fpsLabel")
        {
            X = 60,
            Y = 1,
            Width = 18,
            Height = 1,
            Text = $"FPS: {_animationManager.CurrentFPS:0.0}",
            Colors = new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue)
        };

        AddChild(_animatedButton);
        AddChild(_statusLabel);
        AddChild(_fpsLabel);
    }

    public override void Update()
    {
        base.Update();

        // Update FPS counter every 500ms
        _frameCount++;
        if ((DateTime.Now - _lastFpsUpdate).TotalMilliseconds >= 500)
        {
            _fpsLabel.Text =
                $"Animations: {_animationManager.ActiveAnimations} | FPS: {_animationManager.CurrentFPS:0.0}";
            _lastFpsUpdate = DateTime.Now;
            _frameCount = 0;
        }
    }

    private void OnAnimatedButtonClick()
    {
        _clickCount++;
        _statusLabel.Text = $"Button clicked {_clickCount} times with animations!";

        // Change button text randomly for visual feedback
        var texts = new[] { "Wow!", "Amazing!", "So Smooth!", "Beautiful!", "Excellent!" };
        var randomText = texts[new Random().Next(texts.Length)];
        _animatedButton.Text = randomText;
    }
}