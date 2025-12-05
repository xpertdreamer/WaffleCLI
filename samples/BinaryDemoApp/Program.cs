using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

// Configure logging
TuiLogger.EnableLogging = true;
TuiLogger.QuietMode = true;
TuiLogger.LogFile = "tui-modern.log";
TuiLogger.ClearLog();

TuiLogger.LogInfo("=== Modern Binary Launcher Demo ===");

try
{
    Console.WriteLine("🚀 Initializing Modern Binary Launcher Demo...");
    
    // Load settings
    var settingsManager = new SettingsManager("tui-settings.json");
    if (!File.Exists("tui-settings.json"))
    {
        Console.WriteLine("⚙️ Creating default settings...");
        settingsManager.CreateDefaultSettingsFile();
    }
    
    // Load binaries configuration
    var binariesManager = new BinariesManager("binaries.json");
    var settings = settingsManager.Settings;
    
    Console.WriteLine("🔨 Building modern TUI application...");
    
    var app = new TuiApplicationBuilder()
        .ConfigureServices(services =>
        {
            TuiLogger.LogInfo("Configuring modern demo services");
            services.AddSingleton(settingsManager);
            services.AddSingleton(binariesManager);
            
            // Modern configuration
            services.AddSingleton<ITuiConfiguration>(new TuiConfiguration
            {
                DefaultTheme = settings.Theme,
                FrameRate = 30, // Smooth framerate
                EnableDoubleBuffering = true,
                EnableInputLogging = false
            });
        })
        .UseRootComponent<BinaryDemoNewApp>() // Use the new modern app
        .Build();
    
    Console.WriteLine("✅ Modern application built!");
    Console.WriteLine($"📐 Window size: {settings.WindowWidth}x{settings.WindowHeight}");
    Console.WriteLine("\n🎮 MODERN CONTROLS:");
    Console.WriteLine("   • Tab/Shift+Tab    - Navigate components");
    Console.WriteLine("   • Ctrl+F           - Focus search");
    Console.WriteLine("   • Ctrl+R           - Refresh binaries");
    Console.WriteLine("   • Ctrl+I           - Import binaries");
    Console.WriteLine("   • Ctrl+V           - Validate binaries");
    Console.WriteLine("   • Enter            - Launch selected binary");
    Console.WriteLine("   • Esc              - Exit application");
    Console.WriteLine("\n📦 FEATURES:");
    Console.WriteLine("   • Modern grid layout with visual guides");
    Console.WriteLine("   • Integrated console output");
    Console.WriteLine("   • Real-time binary validation");
    Console.WriteLine("   • Category filtering and search");
    Console.WriteLine("\nPress any key to start the modern demo...");
    
    Console.ReadKey(true);
    Console.Clear();
    
    // Run the modern application
    app.Run();
    
    Console.WriteLine("👋 Modern demo finished.");
    Console.WriteLine("📋 Log: tui-modern.log");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    TuiLogger.LogError("Modern demo failed", ex);
    Console.Clear();
    Console.WriteLine($"💥 Error in modern demo: {ex.Message}");
    Console.WriteLine($"📋 Check log: {Path.GetFullPath("tui-modern.log")}");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}