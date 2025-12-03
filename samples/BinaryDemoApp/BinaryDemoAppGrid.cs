// File: WaffleCLI.Core.TUI/Components/Primitive/FixedBinaryDemoApp.cs
using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Input;
using WaffleCLI.Core.TUI.Components.Layout;
using WaffleCLI.Core.TUI.Components.Primitive;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;
using WaffleCLI.Core.TUI.Configuration;
using WaffleCLI.Core.TUI.Infrastructure.Logging;

namespace WaffleCLI.Core.TUI.Components.Primitive
{
    /// <summary>
    /// Fixed version of BinaryDemoApp with proper initialization and rendering
    /// </summary>
    public class FixedBinaryDemoApp : GridLayout
    {
        private readonly ConsolePanel _consolePanel;
        private readonly FixedBinaryLauncher _binaryLauncher;
        private readonly ImprovedLabel _header;
        private readonly ImprovedLabel _statusLabel;
        private readonly ImprovedLabel _instructions;
        private readonly Button _importButton;
        private readonly Button _validateButton;
        private readonly BinariesManager _binariesManager;
        private readonly SettingsManager _settingsManager;
        private DateTime _lastStatusUpdate = DateTime.MinValue;
        private const int STATUS_UPDATE_INTERVAL_MS = 5000;
        private bool _initialized = false;
        
        public FixedBinaryDemoApp(BinariesManager binariesManager, SettingsManager settingsManager) 
            : base("fixedBinaryDemoApp")
        {
            _binariesManager = binariesManager ?? throw new ArgumentNullException(nameof(binariesManager));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            
            TuiLogger.LogInfo("FixedBinaryDemoApp constructor started");
            
            // Set minimum dimensions
            Width = 100;
            Height = 30;
            
            BackgroundColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkBlue);
            Padding = 1;
            HorizontalSpacing = 1;
            VerticalSpacing = 0;
            
            // Simple grid: 5 rows, 12 columns
            // Row 0: Header (1)
            // Row 1: Main area (star)
            // Row 2: Buttons (3)
            // Row 3: Status (1)
            // Row 4: Instructions (1)
            
            // Columns:
            // 0-7: BinaryLauncher (66%)
            // 8-11: ConsolePanel (34%)
            
            // Add row definitions
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 1 });    // Header
            AddRow(new GridDefinition { Type = GridUnitType.Star, Value = 1 });     // Main area
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 3 });    // Buttons (height 3)
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 1 });    // Status
            AddRow(new GridDefinition { Type = GridUnitType.Pixel, Value = 1 });    // Instructions
            
            // Add column definitions
            for (int i = 0; i < 8; i++) // 8 columns for BinaryLauncher
            {
                AddColumn(new GridDefinition { Type = GridUnitType.Star, Value = 2 });
            }
            for (int i = 0; i < 4; i++) // 4 columns for ConsolePanel
            {
                AddColumn(new GridDefinition { Type = GridUnitType.Star, Value = 1 });
            }
            
            TuiLogger.LogInfo("Creating UI components");
            
            // Create header
            _header = new ImprovedLabel("header")
            {
                Text = "🚀 WaffleCLI TUI - Binary Launcher 🚀",
                Colors = new ColorScheme(ConsoleColor.Yellow, ConsoleColor.DarkBlue),
                TextAlignment = TextAlignment.Center
            };
            
            // Create console panel
            _consolePanel = new ConsolePanel("consolePanel")
            {
                Prompt = "> ",
                ShowPrompt = true,
                NormalColors = new ColorScheme(ConsoleColor.White, ConsoleColor.Black),
                Width = 70
            };
            
            // Create BinaryLauncher
            _binaryLauncher = new FixedBinaryLauncher("fixedBinaryLauncher", binariesManager, _consolePanel);
            
            // Create buttons
            _importButton = new Button("importButton")
            {
                Text = "📁 Import",
                OnClick = ImportBinaries,
                NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkCyan),
                Width = 10,
                Height = 3
            };
            
            _validateButton = new Button("validateButton")
            {
                Text = "✅ Validate",
                OnClick = ValidateBinaries,
                NormalColors = new ColorScheme(ConsoleColor.Black, ConsoleColor.DarkYellow),
                Width = 10,
                Height = 3
            };
            
            // Create status
            _statusLabel = new ImprovedLabel("statusLabel")
            {
                Text = GetStatusMessage(),
                Colors = new ColorScheme(ConsoleColor.Green, ConsoleColor.DarkBlue),
                TextAlignment = TextAlignment.Left
            };
            
            // Create instructions
            _instructions = new ImprovedLabel("instructions")
            {
                Text = "Tab:Navigate • Enter:Launch • Ctrl+F:Search • Ctrl+←→:Categories • Esc:Exit",
                Colors = new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue),
                TextAlignment = TextAlignment.Center
            };
            
            // Add components to grid
            AddChild(_header);
            SetChildPosition(_header, 0, 0, 12, 1);
            
            AddChild(_binaryLauncher);
            SetChildPosition(_binaryLauncher, 0, 1, 5, 1);
            
            AddChild(_consolePanel);
            SetChildPosition(_consolePanel, 5, 1, 12, 1);
            
            AddChild(_importButton);
            SetChildPosition(_importButton, 0, 2, 2, 1);
            
            AddChild(_validateButton);
            SetChildPosition(_validateButton, 2, 2, 2, 1);
            
            AddChild(_statusLabel);
            SetChildPosition(_statusLabel, 0, 3, 12, 1);
            
            AddChild(_instructions);
            SetChildPosition(_instructions, 0, 4, 12, 1);
            
            // Perform initial layout
            DoLayout();
            _initialized = true;
            
            // Initial console messages
            _consolePanel.AddOutputLine("✅ Binary Launcher Initialized");
            _consolePanel.AddOutputLine($"📁 Config: {Path.GetFullPath(binariesManager.ConfigPath)}");
            _consolePanel.AddOutputLine($"📦 Binaries loaded: {binariesManager.GetEnabledBinaries().Count}");
            
            TuiLogger.LogInfo($"FixedBinaryDemoApp initialized: {Width}x{Height}");
        }
        
        private string GetStatusMessage()
        {
            var binaries = _binariesManager.GetEnabledBinaries();
            return $"✅ Ready | Binaries: {binaries.Count} | {DateTime.Now:HH:mm:ss}";
        }
        
        private void ImportBinaries()
        {
            _consolePanel.AddOutputLine("📁 Import binaries from current directory...");
            
            try
            {
                int imported = _binariesManager.ImportFromDirectory(Directory.GetCurrentDirectory(), "Imported");
                _consolePanel.AddOutputLine($"✅ Imported {imported} binaries");
                UpdateStatus();
            }
            catch (Exception ex)
            {
                _consolePanel.AddOutputLine($"❌ Import failed: {ex.Message}", true);
            }
        }
        
        private void ValidateBinaries()
        {
            _consolePanel.AddOutputLine("🔍 Validating binaries...");
            
            var results = _binariesManager.ValidateAllBinaries();
            int validCount = results.Count(r => r.IsValid);
            
            if (validCount == results.Count)
            {
                _consolePanel.AddOutputLine($"✅ All {validCount} binaries are valid!");
            }
            else
            {
                _consolePanel.AddOutputLine($"⚠️  {validCount}/{results.Count} binaries valid", true);
            }
            
            UpdateStatus();
        }
        
        private void UpdateStatus()
        {
            _statusLabel.Text = GetStatusMessage();
            _lastStatusUpdate = DateTime.Now;
        }
        
        public override void Update()
        {
            if (!_initialized) return;
            
            // Update status every 5 seconds
            if ((DateTime.Now - _lastStatusUpdate).TotalMilliseconds >= STATUS_UPDATE_INTERVAL_MS)
            {
                UpdateStatus();
            }
            
            base.Update();
        }
        
        public override void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible || !_initialized) return;
            
            // Draw background
            int absX = AbsoluteX;
            int absY = AbsoluteY;
            
            renderEngine.FillRectangle(absX, absY, Width, Height, ' ', BackgroundColors);
            
            // Draw double border around entire application
            renderEngine.DrawBox(absX, absY, Width, Height, BorderStyle.Double, 
                new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue));
            
            // Render children
            base.Render(renderEngine);
        }
    }
}