// File: Program.cs (обновленная версия)

using Microsoft.Extensions.DependencyInjection;
using WaffleCLI.Abstractions.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Application;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

// Enable logging
TuiLogger.EnableLogging = true;
TuiLogger.QuietMode = true;
TuiLogger.LogFile = "tui-debug.log";
TuiLogger.ClearLog();

TuiLogger.LogInfo("=== WaffleCLI TUI Framework with Binary Launcher ===");

try
{
    Console.WriteLine("🚀 Initializing WaffleCLI TUI Framework...");
    TuiLogger.LogInfo("Application starting");
    
    // Load settings
    Console.WriteLine("📝 Loading settings...");
    var settingsManager = new SettingsManager("tui-settings.json");
    
    if (!File.Exists("tui-settings.json"))
    {
        Console.WriteLine("⚙️ Using default settings");
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
            
            services.AddSingleton<ITuiConfiguration>(new TuiConfiguration
            {
                DefaultTheme = settings.Theme,
                FrameRate = 30,
                EnableDoubleBuffering = settings.EnableDoubleBuffering,
                EnableInputLogging = false
            });
        })
        .UseRootComponent<BinaryDemoApp>()
        .Build();

    Console.WriteLine("✅ Application built successfully!");
    Console.WriteLine("\n🎮 Controls:");
    Console.WriteLine("   • Tab/Shift+Tab - Navigate between components");
    Console.WriteLine("   • Arrow keys - Navigate lists");
    Console.WriteLine("   • Enter - Launch selected binary");
    Console.WriteLine("   • Ctrl+F - Focus search");
    Console.WriteLine("   • Ctrl+Left/Right - Navigate categories");
    Console.WriteLine("   • Type - Search binaries or input in console");
    Console.WriteLine("   • Esc - Exit application");
    Console.WriteLine("\nPress any key to start the application...");
    
    TuiLogger.LogInfo("Application ready, waiting for user input");
    Console.ReadKey(true);
    
    TuiLogger.LogInfo("Starting application main loop");
    Console.Clear();
    
    app.Run();
    
    TuiLogger.LogInfo("Application exited normally");
    
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

public class BinaryDemoApp : Panel
{
    private readonly ConsolePanel _consolePanel;
    private readonly BinaryLauncher _binaryLauncher;
    private readonly Label _header;
    private readonly Label _statusLabel;
    private readonly Label _instructions;
    private readonly BinariesManager _binariesManager;
    private readonly Button _importButton;
    private readonly Button _validateButton;

    public BinaryDemoApp(BinariesManager binariesManager) : base("mainApp")
    {
        _binariesManager = binariesManager ?? throw new ArgumentNullException(nameof(binariesManager));
        
        TuiLogger.LogInfo("BinaryDemoApp constructor started");
        
        try
        {
            Width = Math.Max(100, Console.WindowWidth);
            Height = Math.Max(35, Console.WindowHeight);
            TuiLogger.LogInfo($"BinaryDemoApp dimensions: {Width}x{Height}");
        }
        catch (Exception ex)
        {
            TuiLogger.LogError("Failed to get console dimensions, using defaults", ex);
            Width = 120;
            Height = 35;
        }

        BackgroundColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);
        Border = BorderStyle.Double;
        BorderColors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);

        TuiLogger.LogInfo("Creating UI components");

        // Create header
        _header = new Label("header")
        {
            X = 2,
            Y = 2,
            Width = Math.Max(10, Width - 4),
            Height = 1,
            Text = "🚀 WaffleCLI TUI - Binary Launcher System 🚀",
            Colors = new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue)
        };

        // Create console panel
        _consolePanel = new ConsolePanel("consolePanel")
        {
            X = Width - 62,
            Y = 5,
            Width = 60,
            Height = Height - 10,
            Prompt = "> ",
            ShowPrompt = true
        };

        // Create binary launcher
        _binaryLauncher = new BinaryLauncher("binaryLauncher", binariesManager, _consolePanel)
        {
            X = 2,
            Y = 5,
            Width = Width - 70,
            Height = Height - 15
        };

        // Create utility buttons
        _importButton = new Button("importButton")
        {
            X = 2,
            Y = Height - 8,
            Width = 20,
            Height = 3,
            Text = "📁 Import Binaries",
            OnClick = ImportBinaries,
            NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkCyan)
        };

        _validateButton = new Button("validateButton")
        {
            X = 25,
            Y = Height - 8,
            Width = 20,
            Height = 3,
            Text = "✅ Validate All",
            OnClick = ValidateBinaries,
            NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkYellow)
        };

        // Status label
        _statusLabel = new Label("statusLabel")
        {
            X = 2,
            Y = Height - 4,
            Width = Width - 4,
            Height = 1,
            Text = GetStatusMessage(),
            Colors = new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue)
        };

        // Instructions
        _instructions = new Label("instructions")
        {
            X = 2,
            Y = Height - 2,
            Width = Width - 4,
            Height = 2,
            Text = "Use BinaryLauncher (left) to select and run binaries. Console (right) shows output. Press Tab to navigate.",
            Colors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue)
        };

        // Add components
        AddChild(_header);
        AddChild(_binaryLauncher);
        AddChild(_consolePanel);
        AddChild(_importButton);
        AddChild(_validateButton);
        AddChild(_statusLabel);
        AddChild(_instructions);

        // Initial console message
        _consolePanel.AddOutputLine("✅ Binary Launcher System Ready!");
        _consolePanel.AddOutputLine($"📁 Configuration: {Path.GetFullPath(binariesManager.ConfigPath)}");
        _consolePanel.AddOutputLine($"📦 Loaded {binariesManager.GetEnabledBinaries().Count} binaries");
        _consolePanel.AddOutputLine("");
        _consolePanel.AddOutputLine("💡 Tips:");
        _consolePanel.AddOutputLine("  • Select binary and press Enter to launch");
        _consolePanel.AddOutputLine("  • Use search to find binaries");
        _consolePanel.AddOutputLine("  • Categories: Ctrl+Left/Right");
        _consolePanel.AddOutputLine("  • Direct commands: start <exe> [args]");

        TuiLogger.LogInfo("BinaryDemoApp constructor completed");
    }

    private string GetStatusMessage()
    {
        var binaries = _binariesManager.GetEnabledBinaries();
        return $"✅ Ready | {binaries.Count} binaries available | {DateTime.Now:HH:mm:ss}";
    }

    private void ImportBinaries()
    {
        _consolePanel.AddOutputLine("📁 Import binaries from directory:");
        _consolePanel.AddOutputLine("Enter directory path (or drag & drop):");
        
        // In a real implementation, you would show a dialog or input box
        // For now, we'll simulate with a fixed directory
        string importDir = Directory.GetCurrentDirectory();
        
        try
        {
            int imported = _binariesManager.ImportFromDirectory(importDir, "Imported");
            _consolePanel.AddOutputLine($"✅ Imported {imported} binaries from {importDir}");
            _statusLabel.Text = GetStatusMessage();
        }
        catch (Exception ex)
        {
            _consolePanel.AddOutputLine($"❌ Import failed: {ex.Message}", true);
        }
    }

    private void ValidateBinaries()
    {
        _consolePanel.AddOutputLine("🔍 Validating all binaries...");
        
        var results = _binariesManager.ValidateAllBinaries();
        int validCount = results.Count(r => r.IsValid);
        int invalidCount = results.Count - validCount;
        
        _consolePanel.AddOutputLine($"📊 Validation results: {validCount} valid, {invalidCount} invalid");
        
        foreach (var result in results.Where(r => !r.IsValid))
        {
            _consolePanel.AddOutputLine($"❌ {result.Binary.Name}:", true);
            foreach (var error in result.Errors)
            {
                _consolePanel.AddOutputLine($"   • {error}", true);
            }
        }
        
        if (invalidCount == 0)
        {
            _consolePanel.AddOutputLine("✅ All binaries are valid!");
        }
    }

    public override void Update()
    {
        // Update status every 5 seconds
        if (DateTime.Now.Second % 5 == 0)
        {
            _statusLabel.Text = GetStatusMessage();
        }
        
        base.Update();
    }
}