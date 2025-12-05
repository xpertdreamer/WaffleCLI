using WaffleCLI.Abstractions.TUI.Components.Interfaces;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Components;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

/// <summary>
/// Modern demo application using SimpleGridLayout
/// </summary>
public class BinaryDemoNewApp : SimpleGridLayout
{
    private readonly BinariesManager _binariesManager;
    private readonly SettingsManager _settingsManager;

    private BinaryLauncherNew _binaryLauncher;
    private ConsolePanel _consolePanel;
    private ILabel _headerLabel;
    private ILabel _instructionsLabel;
    private IButton _importButton;
    private IButton _validateButton;
    private ILabel _footerLabel;

    private DateTime _lastUpdate = DateTime.Now;
    private const int UPDATE_INTERVAL_SECONDS = 5;

    /// <summary>
    /// Initializes a new BinaryDemoNewApp
    /// </summary>
    public BinaryDemoNewApp(BinariesManager binariesManager, SettingsManager settingsManager)
        : base("binaryDemoNewApp")
    {
        _binariesManager = binariesManager ?? throw new ArgumentNullException(nameof(binariesManager));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));

        TuiLogger.LogInfo("BinaryDemoNewApp initializing");

        // Configure main grid (2x2 for launcher + console, plus header/footer rows)
        Columns = 12;
        Rows = 6;

        // Use console dimensions with fallback
        try
        {
            Width = Math.Max(100, Console.WindowWidth);
            Height = Math.Max(30, Console.WindowHeight);
        }
        catch
        {
            Width = 120;
            Height = 35;
        }

        BackgroundColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);
        Padding = 1;
        HorizontalSpacing = 1;
        VerticalSpacing = 0;

        InitializeComponents();
        LayoutComponents();

        // Initial console message
        _consolePanel.AddOutputLine("✅ Modern Binary Launcher Initialized");
        _consolePanel.AddOutputLine($"📁 Config: {Path.GetFullPath(_binariesManager.ConfigPath)}");
        _consolePanel.AddOutputLine($"📦 Binaries loaded: {_binariesManager.GetEnabledBinaries().Count}");

        TuiLogger.LogInfo($"BinaryDemoNewApp initialized: {Width}x{Height}");
    }

    private void InitializeComponents()
    {
        // Create console panel first (launcher needs it)
        _consolePanel = ComponentFactory.CreateConsolePanel("console", "> ", true)
            .WithColors(new ColorScheme(ConsoleColor.White, ConsoleColor.Black));

        // Create binary launcher with console panel
        _binaryLauncher = new BinaryLauncherNew("binaryLauncherNew", _binariesManager, _consolePanel);

        // Create other UI components
        _headerLabel = ComponentFactory.CreateLabel("header", "🚀 WAFFLECLI MODERN TUI DEMO 🚀")
            .WithColors(new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue));

        _instructionsLabel = ComponentFactory.CreateLabel("instructions",
                "Tab:Navigate • Enter:Launch • Ctrl+F:Search • Ctrl+R:Refresh • Esc:Exit")
            .WithColors(new ColorScheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue));

        _importButton = ComponentFactory.CreateButton("import", "📁 Import", ImportBinaries)
            .WithSize(12, 2)
            .WithColors(new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkCyan));

        _validateButton = ComponentFactory.CreateButton("validate", "✅ Validate", ValidateBinaries)
            .WithSize(12, 2)
            .WithColors(new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkYellow));

        _footerLabel = ComponentFactory.CreateLabel("footer", GetFooterText())
            .WithColors(new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue));
    }

    private void LayoutComponents()
    {
        // Row 0: Header (spans all 12 columns)
        AddChild(_headerLabel, 0, 0, 12, 1);

        // Row 1-4: Main content area (4 rows tall)
        // Launcher takes 8 columns, console takes 4 columns
        AddChild(_binaryLauncher, 0, 1, 8, 4);
        AddChild(_consolePanel, 8, 1, 4, 4);

        // Row 5: Action buttons
        AddChild(_importButton, 0, 5, 2, 1);
        AddChild(_validateButton, 2, 5, 2, 1);

        // Row 5: Instructions (center)
        AddChild(_instructionsLabel, 4, 5, 4, 1);

        // Row 5: Footer (right side)
        AddChild(_footerLabel, 8, 5, 4, 1);
    }

    private string GetFooterText()
    {
        var binaries = _binariesManager.GetEnabledBinaries();
        return $"{binaries.Count} binaries • {DateTime.Now:HH:mm:ss}";
    }

    private void ImportBinaries()
    {
        _consolePanel.AddOutputLine("📁 Importing binaries from current directory...");

        try
        {
            int imported = _binariesManager.ImportFromDirectory(Directory.GetCurrentDirectory(), "Imported");
            _consolePanel.AddOutputLine($"✅ Imported {imported} binaries");
            UpdateFooter();
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

        if (validCount == results.Count)
        {
            _consolePanel.AddOutputLine($"✅ All {validCount} binaries are valid!");
        }
        else
        {
            _consolePanel.AddOutputLine($"⚠️  {validCount}/{results.Count} binaries valid", true);

            foreach (var result in results.Where(r => !r.IsValid).Take(3))
            {
                _consolePanel.AddOutputLine($"  • {result.Binary.Name}: {string.Join(", ", result.Errors.Take(2))}",
                    true);
            }
        }

        UpdateFooter();
    }

    private void UpdateFooter()
    {
        _footerLabel.Text = GetFooterText();
    }

    /// <summary>
    /// Updates the application state
    /// </summary>
    public override void Update()
    {
        base.Update();

        // Update footer every N seconds
        if ((DateTime.Now - _lastUpdate).TotalSeconds >= UPDATE_INTERVAL_SECONDS)
        {
            UpdateFooter();
            _lastUpdate = DateTime.Now;
        }
    }

    /// <summary>
    /// Renders the demo application
    /// </summary>
    public override void Render(IRenderEngine renderEngine)
    {
        if (!IsVisible) return;

        int absX = AbsoluteX;
        int absY = AbsoluteY;

        // Draw main background
        renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);

        // Draw outer border with double style
        renderEngine.DrawBox(absX, absY, Width, Height, BorderStyle.Double,
            new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue));

        // Draw separator between launcher and console
        int separatorX = absX + (Width * 8 / 12);
        var separatorColor = new ColorScheme(ConsoleColor.DarkCyan, ConsoleColor.DarkBlue);

        for (int y = absY + 1; y < absY + Height - 1; y++)
        {
            renderEngine.DrawChar(separatorX, y, '│', separatorColor);
        }

        // Draw separator above buttons
        int buttonSeparatorY = absY + Height - 6;
        for (int x = absX + 1; x < absX + Width - 1; x++)
        {
            renderEngine.DrawChar(x, buttonSeparatorY, '─', separatorColor);
        }

        // Render children
        base.Render(renderEngine);
    }

    /// <summary>
    /// Handles global input for the demo
    /// </summary>
    public bool HandleGlobalInput(InputEvent inputEvent)
    {
        // Try the launcher first
        if (_binaryLauncher.HandleInput(inputEvent))
        {
            return true;
        }

        // Handle demo-specific shortcuts
        if (inputEvent.Key == ConsoleKey.I && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
        {
            ImportBinaries();
            return true;
        }

        if (inputEvent.Key == ConsoleKey.V && inputEvent.Modifiers.HasFlag(KeyModifiers.Control))
        {
            ValidateBinaries();
            return true;
        }

        return false;
    }
}