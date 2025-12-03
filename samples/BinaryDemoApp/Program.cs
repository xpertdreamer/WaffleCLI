using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

// Configure logging
TuiLogger.EnableLogging = true;
TuiLogger.QuietMode = true;
TuiLogger.LogFile = "tui-fixed.log";
TuiLogger.ClearLog();

TuiLogger.LogInfo("=== WaffleCLI TUI Framework (Fixed Version) ===");

try
{
    Console.WriteLine("🚀 Initializing WaffleCLI TUI Framework (Fixed)...");
    
    // Load settings
    Console.WriteLine("📝 Loading settings...");
    var settingsManager = new SettingsManager("tui-settings.json");
    
    if (!File.Exists("tui-settings.json"))
    {
        Console.WriteLine("⚙️ Creating default settings...");
        settingsManager.CreateDefaultSettingsFile();
    }

    // Load binaries configuration
    Console.WriteLine("📦 Loading binaries configuration...");
    var binariesManager = new BinariesManager("binaries.json");
    
    var settings = settingsManager.Settings;

    Console.WriteLine("🔨 Building TUI application...");
    
    var app = new TuiApplicationBuilder()
        .ConfigureServices(services =>
        {
            TuiLogger.LogInfo("Configuring services");
            services.AddSingleton(settingsManager);
            services.AddSingleton(binariesManager);
            
            // Apply configuration from settings
            services.AddSingleton<ITuiConfiguration>(new TuiConfiguration
            {
                DefaultTheme = settings.Theme,
                FrameRate = 30,
                EnableDoubleBuffering = settings.EnableDoubleBuffering,
                EnableInputLogging = false
            });
        })
        .UseRootComponent<FixedBinaryDemoApp>() // Use fixed version
        .Build();

    Console.WriteLine("✅ Application built successfully!");
    Console.WriteLine($"📐 Using window size: {settings.WindowWidth}x{settings.WindowHeight}");
    Console.WriteLine("\n🎮 Controls:");
    Console.WriteLine("   • Tab/Shift+Tab - Navigate");
    Console.WriteLine("   • Enter - Launch selected binary");
    Console.WriteLine("   • Ctrl+F - Focus search");
    Console.WriteLine("   • Ctrl+←→ - Navigate categories");
    Console.WriteLine("   • Esc - Exit");
    Console.WriteLine("\nPress any key to start...");
    
    Console.ReadKey(true);
    Console.Clear();
    
    // Run application
    app.Run();
    
    Console.WriteLine("👋 Application finished.");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    TuiLogger.LogError("Application failed", ex);
    Console.Clear();
    Console.WriteLine($"💥 Error: {ex.Message}");
    Console.WriteLine($"📋 Log: {Path.GetFullPath("tui-fixed.log")}");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}